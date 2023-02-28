using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.ModelBinding;
using DevExpress.Data.Filtering.Helpers;
using DevExpress.Entity.Model.Metadata;
using DevExpress.XtraPrinting.Native;
using Microsoft.Ajax.Utilities;

namespace WebDtt.Models.Extentions
{
    public class ValidationModels
    {
        private AISExam_testingEntities _context = new AISExam_testingEntities()
        {
            Configuration = { LazyLoadingEnabled = false }
        };

        public void ValidateRulesForStationExams(StationExam model)
        {
            List<StationExam> stationExams;

            var station = _context.Stations.FirstOrDefault(s => s.StationID == model.StationID);
            var examDate = _context.Exams.FirstOrDefault(e => e.ExamID == model.ExamID).TestDateTime;

            if (model.StationExamID != 0)
            {
                stationExams = _context.StationExams
                    .Where(se => se.StationID == model.StationID)
                    .Where(se => se.ExamStartupTime == model.ExamStartupTime)
                    .Where(se => se.StationExamID != model.StationExamID)
                    //.Exclude(model)
                    .ToList();
            }
            else
            {
                stationExams = _context.StationExams
                    .Where(se => se.StationID == model.StationID)
                    .Where(se => se.ExamStartupTime == model.ExamStartupTime)
                    .Where(se =>se.Exam.TestDateTime == examDate)
                    //.Exclude(model)
                    .ToList();
            }

            //проверка на количество незарезервированных мест в пункте на конкретное время (adding, editing)

            var reservedPlaces=0;

            reservedPlaces = stationExams.Count == 0 ? 0 : stationExams.Sum(stationExam => stationExam.ReservedCapacity);

            if (model.ReservedCapacity > station.Capacity - reservedPlaces)
                throw new Exception(
                    $"Недостаточно свободных мест в ППЭ {station.StationViewName}. " +
                    $"Количество зарезервированных мест на выбранный экзамен на время {model.ExamStartupTime} " +
                    $"превышает оставшееся количество мест в выбранном ППЭ на выбранное время. " +
                    $"Осталось свободных мест в ППЭ {station.StationViewName} без учета назначаемого экзамена " +
                    $"составляет {station.Capacity - reservedPlaces}");

        }



    }
}