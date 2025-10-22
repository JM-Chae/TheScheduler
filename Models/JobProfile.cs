using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheScheduler.Models
{
    public class JobProfile
    {
        public required int Id { get; set; }
        public required string Name { get; set; } // 정규직, 파트타임, 관리자 등

        // 월간 상하한 근무 시간
        public int? MaxMonthlyWorkTime { get; set; }
        public int? MinMonthlyWorkTime { get; set; }

        // 주간 상하한 근무시간
        public int? MaxWeeklyWorkTime { get; set; }
        public int? MinWeeklyWorkTime { get; set; }


        // 주간 상하한 휴일
        public int? MaxWeeklyOffDay { get; set; }
        public int? MinWeeklyOffDay { get; set; }

        // 주말 근무 상한
        public int? MaxHolidaysWork {  get; set; }
    }
}
