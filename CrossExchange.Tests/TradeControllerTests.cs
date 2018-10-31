using CrossExchange.Controller;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossExchange.Tests
{
    public class TradeControllerTests : BaseAutoMockedTest
    {
        private readonly TradeController _tradeController;

        public TradeControllerTests()
        {
            _tradeController = Mocker.Create<TradeController>();
        }

        #region GetAllTrading
        [Test]
        public void GetAllTradings_ShouldReturnAllTrades()
        {
            //Creating trades
            var trades = Enumerable.Repeat(new Trade() { Action = "BUY", NoOfShares = 50, Symbol = "ABC", PortfolioId = 1 }, 5);

            //Mocking
            GetMock<ITradeRepository>().Setup(x => x.Query()).Returns(trades.AsQueryable());

            //Testing
            var okObjectResult = _tradeController.GetAllTradings(1) as OkObjectResult;
            Assert.AreEqual(okObjectResult.Value, trades);
        }
        #endregion

        #region Buy
        [Test]
        public async Task Buy_InvalidPortfolioShouldReturnBadRequest()
        {
            //Mocking
            GetMock<IPortfolioRepository>().Setup(x => x.GetAsync(1)).Returns(Task.FromResult((Portfolio)null));

            //Testing
            var badRequestObjectResult = await Task.FromResult(_tradeController.Buy(new TradeModel() { })).Result as BadRequestObjectResult;
            Assert.AreEqual(badRequestObjectResult.Value, "There is no such kind of registered portfolio.");
        }

        [Test]
        public async Task Buy_InvalidShareShouldReturnBadRequest()
        {
            //Creating an objects
            var shares = Enumerable.Repeat(new HourlyShareRate() { Symbol = "ABC", Rate = 90 }, 5).AsQueryable();

            //Mocking
            GetMock<IPortfolioRepository>().Setup(x => x.GetAsync(1)).Returns(Task.FromResult(new Portfolio() { Id = 1, Name = "Ahror" }));
            GetMock<IShareRepository>().Setup(x => x.Query()).Returns(shares);

            //Testing
            var badRequestObjectResult = await Task.FromResult(_tradeController.Buy(new TradeModel() { PortfolioId = 1 })).Result as BadRequestObjectResult;
            Assert.AreEqual(badRequestObjectResult.Value, "There is no such kind of registered share.");
        }

        [Test]
        public async Task Buy_ValidShareAndPortfolioShouldReturnCreatedResult()
        {
            //Creating objects
            Portfolio portFolio = new Portfolio() { Id = 1, Name = "Ahror", Trades = new List<Trade>() };
            var shares = Enumerable.Repeat(new HourlyShareRate() { Symbol = "ABC", Rate = 90 }, 5).AsQueryable();

            //Mocking
            GetMock<IPortfolioRepository>().Setup(x => x.GetAsync(1)).Returns(Task.FromResult(portFolio));
            GetMock<IShareRepository>().Setup(x => x.Query()).Returns(shares);

            //Testing
            var createdResult = await Task.FromResult(_tradeController.Buy(new TradeModel() { Action = "BUY", NoOfShares = 10, PortfolioId = 1, Symbol = "ABC" })).Result as CreatedResult;
            Assert.AreEqual(createdResult.Value, portFolio.Trades.LastOrDefault());
        }
        #endregion

        #region Sell
        [Test]
        public async Task Sell_InvalidShareShouldReturnBadRequest()
        {
            //Creating an objects
            var shares = Enumerable.Repeat(new HourlyShareRate() { Symbol = "ABC", Rate = 90, TimeStamp = DateTime.Now }, 5).AsQueryable();

            GetMock<IShareRepository>().Setup(x => x.Query()).Returns(shares);

            //Testing
            var badRequestObjectResult = await Task.FromResult(_tradeController.Sell(new TradeModel() { Symbol = "CBA" })).Result as BadRequestObjectResult;
            Assert.AreEqual(badRequestObjectResult.Value, "There is no such kind of registered share.");
        }

        [Test]
        public async Task Sell_InvalidPortfolioShouldReturnBadRequest()
        {
            //Creating an objects
            var shares = Enumerable.Repeat(new HourlyShareRate() { Symbol = "ABC", Rate = 90 }, 5).AsQueryable();

            //Mocking
            GetMock<IShareRepository>().Setup(x => x.Query()).Returns(shares);
            GetMock<IPortfolioRepository>().Setup(x => x.GetAsync(1)).Returns(Task.FromResult((Portfolio)null));

            //Testing
            var badRequestObjectResult = await Task.FromResult(_tradeController.Sell(new TradeModel() { Symbol = "ABC" })).Result as BadRequestObjectResult;
            Assert.AreEqual(badRequestObjectResult.Value, "There is no such kind of registered portfolio.");
        }

        [Test]
        public async Task Sell_PortfolioHasNoShareShouldReturnBadRequest()
        {
            //Creating an objects
            Portfolio portFolio = new Portfolio() { Trades = new List<Trade>() };

            var shares = Enumerable.Repeat(new HourlyShareRate() { Symbol = "ABC", Rate = 90 }, 5).AsQueryable();

            //Mocking
            GetMock<IShareRepository>().Setup(x => x.Query()).Returns(shares);
            GetMock<IPortfolioRepository>().Setup(x => x.GetAsync(1)).Returns(Task.FromResult(portFolio));

            //Testing
            var badRequestObjectResult = await Task.FromResult(_tradeController.Sell(new TradeModel() { Symbol = "ABC", PortfolioId = 1 })).Result as BadRequestObjectResult;
            Assert.AreEqual(badRequestObjectResult.Value, "You do not have this kind of share to sell.");
        }

        [Test]
        public async Task Sell_PortfolioHasNoEnoughShareShouldReturnBadRequest()
        {
            //Creating an objects
            var shares = Enumerable.Repeat(new HourlyShareRate() { Symbol = "ABC", Rate = 90 }, 5).AsQueryable();
            var trades = Enumerable.Repeat(new Trade() { Symbol = "ABC", NoOfShares = 2, Action = "BUY" }, 10).ToList();
            Portfolio portFolio = new Portfolio() { Trades = trades };

            //Mocking
            GetMock<IShareRepository>().Setup(x => x.Query()).Returns(shares);
            GetMock<IPortfolioRepository>().Setup(x => x.GetAsync(1)).Returns(Task.FromResult(portFolio));

            //Testing
            var badRequestObjectResult = await Task.FromResult(_tradeController.Sell(new TradeModel() { Symbol = "ABC", PortfolioId = 1, NoOfShares = 50 })).Result as BadRequestObjectResult;
            Assert.AreEqual(badRequestObjectResult.Value, "There are not enough share to sell.");
        }

        [Test]
        public async Task Sell_EverythingValidShouldReturnCreatedResult()
        {
            //Creating an objects
            var shares = Enumerable.Repeat(new HourlyShareRate() { Symbol = "ABC", Rate = 90 }, 5).AsQueryable();
            var trades = Enumerable.Repeat(new Trade() { Symbol = "ABC", NoOfShares = 2, Action = "BUY" }, 10).ToList();
            Portfolio portFolio = new Portfolio() { Trades = trades };

            //Mocking
            GetMock<IShareRepository>().Setup(x => x.Query()).Returns(shares);
            GetMock<IPortfolioRepository>().Setup(x => x.GetAsync(1)).Returns(Task.FromResult(portFolio));

            //Testing
            var createdResult = await Task.FromResult(_tradeController.Sell(new TradeModel() { Symbol = "ABC", PortfolioId = 1, NoOfShares = 10 })).Result as CreatedResult;
            Assert.AreEqual(createdResult.Value, portFolio.Trades.LastOrDefault());
        }

        #endregion
    }
}
