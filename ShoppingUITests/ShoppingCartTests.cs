using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShoppingUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingUI.Tests
{
    [TestClass()]
    public class ShoppingCartTests
    {
        [TestMethod()]
        public void AddToCartTest()
        {
            // Arrange
            ShoppingCart cart = new ShoppingCart();
            cart.Orders.Add("bananer", 4);

            // Act
            cart.AddToCart("bananer", 1);

            // Assert
            Assert.IsTrue(cart.Orders["bananer"] == 5);
        }
    }
}