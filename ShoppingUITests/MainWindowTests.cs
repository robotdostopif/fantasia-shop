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
    public class MainWindowTests
    {
        [TestMethod()]
        public void LoadProductsFromFileTest()
        {
            // Arrange
            string[] productsFromFile = new string[]
            {
                "Aloe vera är välkänd för sina naturligt välgörande egenskaper, för såväl insidan som utsidan av kroppen. Aloe vera är en vildväxande grön växt med långa, tjocka blad, vars geléliknande innehåll används för att göra aloe vera-gel, aloe vera-juice och aloe vera-dryck.:40:l"
            };
            List<Product> products = new List<Product>();

            // Act
            

            // Assert

            Assert.Fail();
        }
    }
}