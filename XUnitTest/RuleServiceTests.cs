using System;
using Xunit;

namespace AccountManagementSystem.Tests
{
    public class RuleServiceTests
    {
        private readonly RuleService _service;

        public RuleServiceTests()
        {
            _service = new RuleService();
        }

        [Fact]
        public void DisplayInterestRules_ShouldDisplayRules_WhenRulesExist()
        {
            RuleService.rules.Clear();
            RuleService.rules.Add(new InterestRule { RuleId = "1", Date = new DateTime(2025, 01, 01), Rate = 5.0m });
            RuleService.rules.Add(new InterestRule { RuleId = "2", Date = new DateTime(2025, 02, 01), Rate = 4.5m });
            var exception = Record.Exception(() => _service.DisplayInterestRules());
            Assert.Null(exception);
        }

        [Fact]
        public void AuthoriseHandler_ShouldReturnTrue_WhenTypeIsI()
        {
            var type = "I";
            var result = _service.AuthoriseHandler(type);
            Assert.True(result);
        }

        [Fact]
        public void AuthoriseHandler_ShouldReturnFalse_WhenTypeIsNotI()
        {
            var type = "D";
            var result = _service.AuthoriseHandler(type);
            Assert.False(result);
        }

        [Fact]
        public void DisplayInterestRules_ShouldNotDisplayRules_WhenNoRulesExist()
        {
            RuleService.rules.Clear();
            var exception = Record.Exception(() => _service.DisplayInterestRules());
            Assert.Null(exception); 
        }
    }
}
