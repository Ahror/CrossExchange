using Autofac.Extras.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrossExchange.Tests
{
    /// <summary>
    /// Base class for tests
    /// </summary>
    public class BaseAutoMockedTest : IDisposable
    {
        protected AutoMock Mocker { get; }

        protected BaseAutoMockedTest()
        {
            Mocker = AutoMock.GetLoose();
        }

        protected Mock<TDepend> GetMock<TDepend>()
            where TDepend : class
        {
            return Mocker.Mock<TDepend>();
        }
        public void Dispose()
        {
            Mocker.Dispose();
        }
    }
}
