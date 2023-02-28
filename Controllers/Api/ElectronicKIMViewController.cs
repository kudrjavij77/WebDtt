using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.ModelBinding;
using System.Web.Mvc;
using AisExam8.Base;
using AisExam8.ElectronicKIMViewer;
using DevExpress.Utils.DirectXPaint;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebDtt.Models;
using WebDtt.Models.Extentions;



namespace AisExam8.Controllers.Admin
{
    [System.Web.Http.Authorize(Roles = "user")]
    [System.Web.Http.Route("api/ElectronicKIMView/{action}", Name = "ElectronicKIMViewApi")]
    public class ElectronicKIMViewController : ApiController, IElectronicKIMViewer
    {
        private AISExam_testingEntities db = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        private const string sessionPropertyName = "studentExamId";
        /*
         *  по-умолчанию id во всех методах - это ID КИМа (варианта КИМ)
         *  session-id берется из реквеста. (кроме GetTimeLeft - там для ускорения session-id кормится сразу)
         */
        public IExamSessionController ExamSessionController => null;
        [System.Web.Http.HttpGet]
        public async Task<HttpResponseMessage> GetKIMStructure(Guid id)
        {
            var sql =
                await Task.Factory.StartNew(()=> db.ae_ekim_GetKIMStructure(id));
            return Request.CreateResponse(HttpStatusCode.OK, sql);
        }

        [System.Web.Http.HttpGet]
        public async Task<HttpResponseMessage> GetTimeLeft(string sessionId)
        {
            var s =  await KimSessionFactory.Instance.GetAsync(sessionId);
            //if (s.ExamEndTime == null) return Request.CreateResponse(HttpStatusCode.OK, (object)null);
            return Request.CreateResponse(HttpStatusCode.OK, s.GetExamSecondsLeft());
        }

        [System.Web.Http.HttpGet]
        public async Task<HttpResponseMessage> GetTask(Guid id, int taskNumber)
        {
            if (taskNumber > 0)
            {
                var userSessionId = _getCookieValue(sessionPropertyName);
                var s = await _getSession(userSessionId);
                if (s.ExamStartTime == null || s.SessionEnded || s.ExamEndTime < DateTime.Now)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Экзамен ещё не начат или уже прошел");
            }
            var taskNumberFlag = (long)Math.Pow(2, taskNumber-1);
            var sql =
                db.ElectronicKIMTasks.Where(x=> x.ElectronicKIM == id);
            if (taskNumber == 0)
                sql = sql.Where(x => x.TaskNumberFlags == 0);
            else
                sql = sql.Where(x => (x.TaskNumberFlags & taskNumberFlag) > 0);
            sql = sql.OrderBy(x=>x.DisplayOrder);
            var result = await DataSourceLoader.LoadAsync(sql, new DataSourceLoadOptions());
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> StartExam([FromBody] Guid id)
        {
            var userSessionId = _getCookieValue(sessionPropertyName);
            var s = await _getSession(userSessionId);
            if (string.IsNullOrEmpty(s.KimID)) s.KimID = id.ToString();
            //TODO HERE MUST BE ACTUAL EXAM TIME (сделано! добавить продолжительность в екзамены)
            var seID = int.Parse(s.SessionID);
            var duration = db.StudentExams
                .Where(se => se.StudentExamID == seID)
                .Select(x=>x.Exam.Duration)
                .FirstOrDefault();
            if (duration == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "duration is null");
            if (s.ExamStartTime == null) s.StartTest(duration.Value);
            return Request.CreateResponse(HttpStatusCode.OK, s);
        }

        [System.Web.Http.HttpGet]
        public async Task<HttpResponseMessage> FinishExam()
        {
            var userSessionId = _getCookieValue(sessionPropertyName);
            var s = await _getSession(userSessionId);
            var result = s.StopTest();
            if (result == 0)
            {
                await _saveSessionToDb(s);
                return Request.CreateResponse(HttpStatusCode.OK, 3);
            }
            return Request.CreateResponse(HttpStatusCode.OK, result+1);
        }

        private async Task _saveSessionToDb(KimSession session, bool needNewContext = false)
        {
            try
            {
                var _context = needNewContext ? new AISExam_testingEntities() : db;
                var studentExamIDformSession = int.Parse(session.SessionID);
                var studentExam = _context.StudentExams.FirstOrDefault(se => se.StudentExamID == studentExamIDformSession);
                if(studentExam==null) throw new NullReferenceException("studentExam object is null in saveToDb");
                if(studentExam.FinishDateTime==null)
                    studentExam.FinishDateTime = session.ExamEndTime;
                studentExam.Flags = studentExam.Flags | 16;

                foreach (var answerFromView in session.Answers)
                {
                    
                    var answerValue = answerFromView.Value.Substring(0, Math.Min(answerFromView.Value.Length, 1000));
                    var bAnswer = _context.BAnswers.FirstOrDefault(b =>
                        b.StudentExamID == studentExamIDformSession && b.BQuestion.Number == answerFromView.Key - 1);
                    if (bAnswer == null)
                    {
                        var eKim = _context.ElectronicKIMs.FirstOrDefault(e => e.ObjectID == studentExam.ElectronicKIMID);
                        var bQuestion = _context.BQuestions.FirstOrDefault(bq => bq.Number == answerFromView.Key - 1 && bq.KIM == eKim.KIM);
                        if (bQuestion == null) continue;
                        _context.BAnswers.Add(new BAnswer()
                        {
                            BQuestionID = bQuestion.ObjectID,
                            StudentExamID = studentExam.StudentExamID,
                            Value = answerValue,
                            Flags = 4
                        });
                    }
                    else
                    {
                        bAnswer.Value = answerValue;
                    }
                }

                await _context.SaveChangesAsync();
                // декод studentExamId
            }
            catch (Exception e)
            {
                Log.My.Error<ElectronicKIMViewController>(e);
                throw;
            }
        }

        private string _getCookieValue(string name)
        {
            var cookiez = Request.Headers.GetValues("cookie").First();
            if (string.IsNullOrEmpty(cookiez)) return "";
            var arrCookies = cookiez.Split(';').FirstOrDefault(x => x.Contains(name));
            if (string.IsNullOrEmpty(arrCookies))
            {
                //var cookieFromReq = HttpUtility.HtmlDecode(Request.Properties[sessionPropertyName].ToString());
                return "";
            }
            var pos = arrCookies.IndexOf('=');
            var val = arrCookies.Remove(0,pos+1).Trim();
            //Request.Headers.GetCookies(name).FirstOrDefault();
            //if (cookie == null) return string.Empty;

            //TODO:cooky from bd
            //var user = System.Web.HttpContext.Current.GetOwinContext().Authentication.User;
            //var userid = Guid.Parse(user.Claims.Select(u => u.Value).FirstOrDefault() ?? throw new InvalidOperationException());
            //var r = Request;
            //if (string.IsNullOrEmpty(val))
            //{
            //    //var sooka = db.Cookies.Where(x => x.CookieID);
            //}

            return val;
        }

        private void _saveSessionAction(KimSession session)
        {
            Task.Run(()=>_saveSessionToDb(session, true)).Wait();
        }

        [System.Web.Http.HttpGet]
        public async Task<HttpResponseMessage> GetSession(Guid id)
        {
            var userSessionId = _getCookieValue(sessionPropertyName); //Properties[sessionPropertyName];
            if (string.IsNullOrEmpty(userSessionId)) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Нажмите, пожалуйста, клавиши Shift+F5 для перезагрузки сессии");
            var s = await _getSession(userSessionId);
            s.KimID = id.ToString();
            if (s.KimID != id.ToString())
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Не совпадает ID КИМ сохранённой сессии");
            return Request.CreateResponse(HttpStatusCode.OK, s);
        }
        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> SetSession([FromBody] JObject data)
        {
            if (data == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "session data is empty");
            var item = data.ToObject<KimSession>();
            if (item == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "deseralization failure");
            //TODO compare with crypted sessid
            var decodedId = _decodeSessionId(_getCookieValue(sessionPropertyName));
            if(item.SessionID!=decodedId) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "sessId failure");
            var session = await KimSessionFactory.Instance.GetAsync(item.SessionID);
            if(string.IsNullOrEmpty(session.KimID)) session.KimID = item.KimID;
            session.Player.volume = item.Player.volume;
            session.Player.index = item.Player.index;
            if (!session.Player.audioComplete)
            {
                session.Player.audioComplete = item.Player.audioComplete;
                if (session.Player.currentTime < item.Player.currentTime)
                    session.Player.currentTime = item.Player.currentTime;
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }


        private HttpResponseMessage _getMediaStream(string path, string mediaType)
        {
            var headerMediaType = new MediaTypeHeaderValue(mediaType);
            var provider = new LocalFileStorageProvider("D:\\aisexam8filestore");
            var stream = provider.GetStream<ElectronicKIMTask>(path);
            if (Request.Headers.Range != null)
            {
                // Return part
                HttpResponseMessage partialResponse = Request.CreateResponse(HttpStatusCode.PartialContent);
                partialResponse.Content = new ByteRangeStreamContent(stream, Request.Headers.Range, headerMediaType);
                return partialResponse;
            }
            else
            {
                // Return complete 
                HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
                fullResponse.Content = new StreamContent(stream);
                fullResponse.Content.Headers.ContentType = headerMediaType;
                return fullResponse;
            }
        }

        [System.Web.Http.HttpGet]
        public async Task<HttpResponseMessage> GetFile(Guid id, int down = 0)
        {
            var file = await db.ElectronicKIMTaskFiles
                .Select(x => new
                {
                    x.Name,
                    x.FilePath,
                    x.ObjectID,
                    x.ElectronicKIMTask,
                    ContentType = x.ContentType1.Name
                })
                .FirstOrDefaultAsync(x => x.ObjectID == id);

            if (file == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "no file found in database");
            if (file.ContentType.Contains("audio"))
            {
                return _getMediaStream(file.FilePath, file.ContentType);
            }
            var provider = new LocalFileStorageProvider("D:\\aisexam8filestore");
            var content = await provider.Get<ElectronicKIMTask>(file.FilePath);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(content);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
            if (down > 0)
            {
                response.Content.Headers.ContentDisposition.DispositionType = "attachment";
                response.Content.Headers.ContentDisposition.FileName = file.Name;
                
            }
            
            response.Content.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType);
            // response.Headers.ConnectionClose = false;

            return response;
        }

        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> UpdateTask([FromBody] JObject data)
        {
            if (data == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "data is empty");
            try
            {
                var taskType = data["taskType"].Value<int>();
                var taskNumber = data["taskNumber"].Value<int>();
                var sessionId = data["sessionId"].Value<string>();
                var value = data["value"].Value<string>();
                var kimId = data["kimId"].Value<string>();
                var session = await _getSession(_getCookieValue(sessionPropertyName));
                if (session.ExamStartTime == null || session.SessionEnded || session.ExamEndTime < DateTime.Now)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Экзамен ещё не начат или уже прошел");
                if (session.KimID!=kimId) throw new Exception("KimID is different for the session");
                session.SetAnswer(taskNumber, value);
                //session.Answers[taskNumber] = value; - ТАК НЕ ДЕЛАТЬ, НЕ РАБОТАЕТ ДАМПЕР!
                //TODO: А буду ли я сохранять каждый ответ в БД?
            }
            catch (Exception)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "data is incomplete or damaged");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> SaveSession([FromBody] JObject data)
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private string _decodeSessionId(string crypto)
        {
            var data = CryptoPeaceduke.Decrypt(crypto, "BelkaPizda");
            //if (string.IsNullOrEmpty(data)) ;// throw new Exception($"invalid {sessionPropertyName}");
            return data;
        }

        private async Task<KimSession> _getSession(string id)
        {
            var trueId = _decodeSessionId(id);
            KimSessionFactory.SetOnSessionElapsedAction(_saveSessionAction);
            var session = await KimSessionFactory.Instance.GetAsync(trueId) ?? KimSessionFactory.Instance.New(trueId);
            return session;
        }
    }
}
