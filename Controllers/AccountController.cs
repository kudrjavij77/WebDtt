using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using WebDtt.Models;
using WebDtt.Models.Extentions;

namespace WebDtt.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private AISExam_testingEntities _db = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false}
        };
        
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {

            if (ModelState.IsValid)
            {
                var passHash = Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Password)));
                User user = await _db.Users.Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == passHash);

                if (user == null)
                {
                    ModelState.AddModelError("", "Неверный логин или пароль");
                }
                else
                {
                    if (user.EmailConfirmed)
                    {
                        AddClaims(user);
                        return RedirectToLocal(returnUrl);
                    }
                    ModelState.AddModelError("", "Не подтвержден email. Проверьте свой Email.");

                }
            }
            return View(model);
        }
        
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                Role role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "user");
                if (user != null)
                {
                    ModelState.AddModelError("", "Пользователь с таким Email уже существует");
                    return View(model);
                }

                user = new User()
                {
                    UserID = Guid.NewGuid(),
                    Email = model.Email,
                    PasswordHash =
                        Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(model.ConfirmPassword))),
                    Roles = new List<Role>()
                };
                user.Roles.Add(role);
                _db.Users.Add(user);
                _db.Entry(user).State = EntityState.Added;
                

                var code = Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(user.Email)));
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new {userId = user.UserID, code = code},
                    protocol: Request.Url.Scheme);
                EmailServiceSend emailService = new EmailServiceSend();
                await emailService.EmailConfirmedSend(0, user, callbackUrl);

                await _db.SaveChangesAsync();

                return View("DisplayEmail");

                //AddClaims(user);
                //return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null || !(user.EmailConfirmed))
                {
                    return View("ForgotPasswordConfirmation");
                }
                string code = Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(user.Email)));
                var callbackUrl = Url.Action("ResetPassword", "Account",
                    new { userId = user.UserID, code = code }, protocol: Request.Url.Scheme);

                EmailServiceSend emailService = new EmailServiceSend();
                await emailService.EmailForgotPasswordSend(0, user, callbackUrl);

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                // Не показывать, что пользователь не существует
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            _db.Entry(user).State = EntityState.Modified;
            user.PasswordHash =
                Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(model.ConfirmPassword)));
            _db.SaveChanges();

            return RedirectToAction("ResetPasswordConfirmation", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }



        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return View("Error");
            
            var id = Guid.NewGuid();
            if (!Guid.TryParse(userId, out id))
                return View("Error"); 

            User user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == id);
            if (user==null)
                return View("Error");

            if (Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(user.Email)))!=code)
                return View("Error");


            _db.Entry(user).State = EntityState.Modified;
            user.EmailConfirmed = true;
            var result = _db.SaveChanges();

            return View("ConfirmEmail");
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            return Url.IsLocalUrl(returnUrl) ? (ActionResult) Redirect(returnUrl) : RedirectToAction("Index", "Home");
        }

        private void AddClaims(User user)
        {
            ClaimsIdentity claim = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            claim.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString(), ClaimValueTypes.String));
            claim.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email, ClaimValueTypes.String));
            claim.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
                "OWIN Provider", ClaimValueTypes.String));
            if (user.Roles.Count != 0)
                foreach (var r in user.Roles)
                {
                    claim.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, r.RoleName, ClaimValueTypes.String));
                }

            var http = HttpContext;

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //AuthenticationManager.SignOut();
            AuthenticationManager.SignIn(new AuthenticationProperties {IsPersistent = true}, claim);

            var http1 = HttpContext;
            var a = 1;
        }

    }
}