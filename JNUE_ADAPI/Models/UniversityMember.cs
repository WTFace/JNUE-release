using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JNUE_ADAPI.Models
{
    [Table("office365")]
    public class UniversityMember
    {
        // 학번/교번
        [Key]
        public string stnt_numb { get; set; }

        // 재학/재직:1 휴학:2 졸업/퇴직:0
        public int status { get; set; }

        // 성명
        public string stnt_knam { get; set; }

        // Y:사용 N:사용안함또는 졸업
        public string user_used { get; set; }

        // 대상자정보(학생/교직원)
        public string role { get; set; }

        // 최종학적변동일
        public DateTime hcng_date { get; set; }
    }
}