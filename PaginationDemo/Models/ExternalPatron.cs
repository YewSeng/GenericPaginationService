using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PaginationDemo.Models
{
    public class ExternalPatron
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FormID { get; set; }

        public int HighRiskPatronId { get; set; } // Foreign key property
        //public virtual HighRiskPatron HighRiskPatron { get; set; } // for navigation
        public virtual int DocumentID { get; set; } // may change in future
        [Required]
        public required DateTime CreatedDate { get; set; } // date which this form was created
        public string? CreatedByID { get; set; } // ID of the actor who created this form
        public ASPNETUser? CreatedByUser { get; set; } // actor who created this form
        public DateTime? LastModifiedDate { get; set; } // date which this form was last modified
        public string? LastModifiedByID { get; set; } // ID of the actor who last modified this form
        public ASPNETUser? LastModifiedByUser { get; set; } // actor who last modified this form
        public DateTime? SubmittedForApprovalDate { get; set; } // date which this form was submitted for approval
        public string? SubmittedForApprovalByID { get; set; } // ID of the actor who submitted this form for approval
        public ASPNETUser? SubmittedForApprovalByUser { get; set; } // actor who submitted this form for approval
        public DateTime? ApprovedDate { get; set; } // date which this form is approved
        public string? ApprovedByID { get; set; } // ID of the approver who approved this form
        public ASPNETUser? ApprovedByUser { get; set; } // approver who approved this form
        public string? Description { get; set; }
        [Required]
        public Status FormStatus { get; set; }
        public string? FirstName { get; set; } // from external database, filled only when approved
        public string? LastName { get; set; } // from external database, filled only when approved
        public string? FullName => FirstName + " " + LastName;
        public string? Nationality { get; set; } // from external database, filled only when approved
        public Gender? Gender { get; set; } // from external database, filled only when approved
        public DateTime? DateOfBirth { get; set; } // from external database, filled only when approved
        public string? PassportNumber { get; set; } // from external database, filled only when approved
        public string? EmailAddress { get; set; } // from external database, filled only when approved
        public string? ContactNumber { get; set; } // from external database, filled only when approved
        public string? MailingAddress { get; set; } // from external database, filled only when approved
        public string? BillingAddress { get; set; } // from external database, filled only when approved
        public decimal? AverageBetSize { get; set; } // from external database, filled only when approved
        public decimal? WinLoseRatio { get; set; } // from external database, filled only when approved
        public decimal? FrequencyOfVisitsPerMonth { get; set; } // from external database, filled only when approved
        public Boolean? SelfExclusionStatus { get; set; } // true if patron is excluded
        public string? AdditionalInformation { get; set; } // from external database, filled only when approved
        public DateTime? DateJoined { get; set; } // from external database, filled only when approved
    }
}
