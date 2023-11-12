using NUnit.Framework;
using BankingApp; // This is assuming your DebitCard class is in the BankingApp namespace

namespace BankingTest
{
    [TestFixture]
    public class DebitCardTests
    {
        [Test]
        public void Deposit_ValidAmount_IncreasesBalance()
        {
            // Assuming you have a DebitCard class in your BankingApp project
            var debitCard = new DebitCard(new FakeTransactionLogger());

            var initialBalance = debitCard.Balance;
            var depositAmount = 100m;
            debitCard.Deposit(depositAmount);

            Assert.AreEqual(initialBalance + depositAmount, debitCard.Balance);
        }

        // Additional test methods...
    }
}
