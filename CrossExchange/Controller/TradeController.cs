using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CrossExchange.Controller
{
    [Route("api/Trade")]
    public class TradeController : ControllerBase
    {
        private IShareRepository _shareRepository { get; set; }
        private ITradeRepository _tradeRepository { get; set; }
        private IPortfolioRepository _portfolioRepository { get; set; }

        public TradeController(IShareRepository shareRepository, ITradeRepository tradeRepository, IPortfolioRepository portfolioRepository)
        {
            _shareRepository = shareRepository;
            _tradeRepository = tradeRepository;
            _portfolioRepository = portfolioRepository;
        }


        [HttpGet("{portfolioid}")]
        public IActionResult GetAllTradings([FromRoute]int portFolioId)
        {
            var trade = _tradeRepository.Query().Where(x => x.PortfolioId.Equals(portFolioId)).ToList();
            return Ok(trade);
        }



        /*************************************************************************************************************************************
        For a given portfolio, with all the registered shares you need to do a trade which could be either a BUY or SELL trade. For a particular trade keep following conditions in mind:
		BUY:
        a) The rate at which the shares will be bought will be the latest price in the database.
		b) The share specified should be a registered one otherwise it should be considered a bad request. 
		c) The Portfolio of the user should also be registered otherwise it should be considered a bad request. 
                
        SELL:
        a) The share should be there in the portfolio of the customer.
		b) The Portfolio of the user should be registered otherwise it should be considered a bad request. 
		c) The rate at which the shares will be sold will be the latest price in the database.
        d) The number of shares should be sufficient so that it can be sold. 
        Hint: You need to group the total shares bought and sold of a particular share and see the difference to figure out if there are sufficient quantities available for SELL. 

        *************************************************************************************************************************************/

        [HttpPost("buy")]
        public async Task<IActionResult> Buy([FromBody]TradeModel tradeModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Getting portfolio to check if it is registered
            var portFolio = await _portfolioRepository.GetAsync(tradeModel.PortfolioId);
            if (portFolio == null) { return BadRequest("There is no such kind of registered portfolio."); }

            // Getting share to check if it is registered
            var share = _shareRepository.Query().Where(s => s.Symbol == tradeModel.Symbol).OrderByDescending(o => o.TimeStamp).FirstOrDefault();
            if (share == null) { return BadRequest("There is no such kind of registered share."); }

            //Making a new trade
            var trade = new Trade() { Symbol = tradeModel.Symbol, NoOfShares = tradeModel.NoOfShares, PortfolioId = portFolio.Id, Action = "BUY", Price = share.Rate * tradeModel.NoOfShares };
            portFolio.Trades.Add(trade);

            //Updating
            await _portfolioRepository.UpdateAsync(portFolio);

            return Created("Trade", trade);
        }

        [HttpPost("sell")]
        public async Task<IActionResult> Sell([FromBody]TradeModel tradeModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Getting share to check if it is registered
            var share = _shareRepository.Query().Where(s => s.Symbol == tradeModel.Symbol).OrderByDescending(o => o.TimeStamp).FirstOrDefault();
            if (share == null) { return BadRequest("There is no such kind of registered share."); }

            //Getting portfolio to check if it is registered
            var portFolio = await _portfolioRepository.GetAsync(tradeModel.PortfolioId);
            if (portFolio == null) { return BadRequest("There is no such kind of registered portfolio."); }

            //Checking portfolio if it contains this kind of share
            if (!portFolio.Trades.Any(s => s.Symbol == tradeModel.Symbol)) { return BadRequest("You do not have this kind of share to sell."); }

            //Checking portfolio if it contains enough count of share to sell
            if (GetAvailableShareCount(portFolio) < tradeModel.NoOfShares) { return BadRequest("There are not enough share to sell."); }

            //Making a new trade
            var trade = new Trade() { Symbol = tradeModel.Symbol, NoOfShares = tradeModel.NoOfShares, PortfolioId = portFolio.Id, Action = "SELL", Price = share.Rate * tradeModel.NoOfShares };
            portFolio.Trades.Add(trade);

            //Updating
            await _portfolioRepository.UpdateAsync(portFolio);

            return Created("Trade", trade);
        }

        /// <summary>
        /// Getting available share count from portfolio
        /// </summary>
        /// <param name="portFolio"></param>
        /// <returns></returns>
        private int GetAvailableShareCount(Portfolio portFolio)
        {
            return portFolio.Trades.Where(s => s.Action == "BUY").Sum(s => s.NoOfShares) - portFolio.Trades.Where(s => s.Action == "SELL").Sum(s => s.NoOfShares);
        }

    }
}
