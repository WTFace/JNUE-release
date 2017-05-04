using System;
using System.ComponentModel.DataAnnotations;

namespace JNUE_ADAPI.Models
{
    public class StntNumbCheckViewModel
    {
        [Required]
        [Display(Name = "학번 / 교번")]
        public int Stnt_Numb { get; set; }       
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "아이디")]
        [RegularExpression(@"\w+([-+.']\w+)*", ErrorMessage = "아이디는 영문 대소문자로 시작하고, 숫자,'-','_','.'을 포함할 수 있습니다.")]
        public string ID { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0}은(는) {2}자 이상이어야 합니다.", MinimumLength = 7)]
        [DataType(DataType.Password)]
        [Display(Name = "암호")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*\d).{3,15}$", ErrorMessage = "비밀번호는 1개 이상의 영소문자, 숫자를 포함하여 7~15자 이내로 입력하여야 합니다.")] 
        //(?=.*[A-Z])(?=.*[^\da-zA-Z])
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "암호 확인")]
        [Compare("Password", ErrorMessage = "암호와 확인 암호가 일치하지 않습니다.")]
        public string ConfirmPassword { get; set; }
    }
}