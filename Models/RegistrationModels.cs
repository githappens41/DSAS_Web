using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DSAS.Web.Models
{
    // 1. Matches the Flutter 'student_cards' table exactly
    public class RegistrationDto
    {
        [JsonPropertyName("id")]
        public string RegistrationId { get; set; } = string.Empty;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("student_number")]
        public string StudentNumber { get; set; } = string.Empty;

        [JsonPropertyName("programme")]
        public string? Programme { get; set; }

        [JsonPropertyName("photo_url")]
        public string? PhotoUrl { get; set; }

        [JsonPropertyName("proof_url")]
        public string? ProofOfRegistrationUrl { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Pending";

        [JsonPropertyName("rejection_reason")]
        public string? RejectionReason { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? SubmittedDate { get; set; }
    }
  
    public class StaffDto
    {
        [JsonPropertyName("staff_id")]
        public string StaffId { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    public class DashboardViewModel
    {
        public int Submitted { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public List<RegistrationDto> Registrations { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class RegistrationDetailViewModel
    {
        public RegistrationDto Registration { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class RejectViewModel
    {
        [Required(ErrorMessage = "Please provide a reason for rejection.")]
        public string Reason { get; set; } = string.Empty;
    }
}