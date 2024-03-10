using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Proestimator.ViewModel
{
    public class UserFeedbackVM
    {
        [Required(ErrorMessage = "Please Enter Your Feedback")]
        [Display(Name = "Feedback")]
        public string FeedbackText { get; set; }

        public int UserID { get; set; }

        public List<string> ImagesPath { get; set; }
    }
}