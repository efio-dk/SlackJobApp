using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using SlackJobPosterReceiver.API;
using Xunit;

namespace SlackJobPosterReceiver.Tests
{
    public class APITests
    {
        [Fact]
        public async Task ClosePostLeadTest()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   Content = new StringContent("{\"test\":\"TestLead\"}")
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var expectedJson = JObject.Parse("{\"test\":\"TestLead\"}");

            var closePoster = new ClosePoster(httpClient);
            var actualJson = await closePoster.PostLead("DSB");

            Assert.True(JToken.DeepEquals(expectedJson, actualJson));
        }

        [Fact]
        public async Task ClosePostOpportunityTest()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   Content = new StringContent("{\"test\":\"TestOpportunty\"}")
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var expectedJson = JObject.Parse("{\"test\":\"TestOpportunty\"}");

            var closePoster = new ClosePoster(httpClient);
            var actualJson = await closePoster.PostOpportunity("", "", "");

            Assert.True(JToken.DeepEquals(expectedJson, actualJson));
        }

        [Fact]
        public async Task SlackTriggerModalOpen()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var expectedResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            var slackPoster = new SlackPoster(httpClient);
            var actualResponse = await slackPoster.TriggerModalOpen("", "", "");

            Assert.Equal(expectedResponse.StatusCode, actualResponse.StatusCode);
        }

        [Fact]
        public async Task SlackUpdateMessage()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var expectedResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            var slackPoster = new SlackPoster(httpClient);
            var actualResponse = await slackPoster.UpdateMessage(new JObject(), "");

            Assert.Equal(expectedResponse.StatusCode, actualResponse.StatusCode);
        }
    }
}