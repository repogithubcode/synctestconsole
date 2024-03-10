using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Logic.ChatBot
{
    public class ChatBotService : IChatBotService
    {
        //private readonly HttpClient httpClient;
        private const string ApiBaseUrl = "http://127.0.0.1:5000"; // Replace with the actual API URL
        private const int ChatHistoryLimit = 10;

        public ChatBotService()
        {
            //httpClient = new HttpClient();
        }

        public string GetAnswerAsync(string question)
        {
            HttpClient httpClient = new HttpClient();

            // Prepare the JSON data for the API request
            var requestData = new { question = question };
            var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            try
            {
                // Send a POST request to the API endpoint
                HttpResponseMessage response = Task.Run(() => httpClient.PostAsync($"{ApiBaseUrl}/get_answer", jsonContent)).Result;
                response.EnsureSuccessStatusCode();

                // Read the response as JSON
                var responseBody = Task.Run(() => response.Content.ReadAsStringAsync()).Result;
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseBody);

                // Get the answer from the response
                var answer = result.answer.ToString();

                // Store the chat history in the session variable
                AddToChatHistory(question, answer);

                return answer;
            }
            catch (Exception ex)
            {
                // Handle API request error
                // For simplicity, you can log or throw the exception
                string error = ex.Message;
                return "";
            }
        }

        private void AddToChatHistory(string question, string answer)
        {
            // Retrieve or create a new chat history list in the session
            var chatHistory = (List<(string, string)>)System.Web.HttpContext.Current.Session["ChatHistory"];
            if (chatHistory == null)
            {
                chatHistory = new List<(string, string)>();
                System.Web.HttpContext.Current.Session["ChatHistory"] = chatHistory;
            }

            // Add the question and answer to the chat history
            chatHistory.Add((question, answer));

            // Enforce the chat history limit
            if (chatHistory.Count > ChatHistoryLimit)
            {
                chatHistory.RemoveAt(0); // Remove the oldest item
            }
        }
    }
}
