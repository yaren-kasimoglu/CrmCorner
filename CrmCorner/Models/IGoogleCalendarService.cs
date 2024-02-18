using System;
namespace CrmCorner.Models
{
	
        public interface IGoogleCalendarService
        {
            string GetAuthCode();

            Task<GoogleTokenResponse> GetTokens(string code);
            string AddToGoogleCalendar(Calendar googleCalendarReqDTO);
        }
    
}

