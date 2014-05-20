﻿using NUnit.Framework;
using OpenQA.Selenium;
using SaasEcom.Specs.Base;
using TechTalk.SpecFlow;

namespace SaasEcom.Specs.Steps.Website
{
    [Binding]
    public class RegisterSteps
    {
        [Given(@"I have the homepage open")]
        public void GivenIHaveTheHomepageOpen()
        {
            WebBrowser.Current.Navigate().GoToUrl(StepsHelpers.BaseUrl);
            Assert.AreEqual("Home - SAAS Ecom", WebBrowser.Current.Title);
        }

        [When(@"I click on ""(.*)""")]
        public void WhenIClickOn(string buttonId)
        {
            WebBrowser.Current.FindElement(By.Id(buttonId)).Click();
        }

        [Then(@"I should see ""(.*)"" on the screen")]
        public void ThenIShouldSeeOnTheScreen(string p0)
        {
            Assert.AreEqual(p0, WebBrowser.Current.FindElement(By.XPath("/html/body/div[2]/div[1]/p[1]")).Text);
        }

        [Then(@"I see the registration form")]
        public void ThenISeeTheRegistrationForm()
        {
            Assert.AreEqual("Register - SAAS Ecom", WebBrowser.Current.Title);
        }

        [Given(@"I am at the registration page")]
        public void GivenIAmAtTheRegistrationPage()
        {
            WebBrowser.Current.Navigate().GoToUrl(StepsHelpers.BaseUrl + "Account/Register");
            Assert.AreEqual("Register - SAAS Ecom", WebBrowser.Current.Title);
        }

        [When(@"I fill the registration form")]
        public void WhenIFillTheRegistrationForm(Table table)
        {
            foreach (var row in table.Rows)
            {
                var field = row["field"];
                var value = row["value"];
                switch (field)
                {
                    case "SubscriptionPlan":  // Put exceptions here for drop downs, date pickers, radio buttons, etc.
                        WebBrowser.Current.SelectDropDownByValue(By.Id(field), value);
                        break;
                    default:
                        WebBrowser.Current.SetTextForControl(By.Id(field), value);
                        break;
                }
            }
        }

        [When(@"I fill the registration form with invalid data")]
        public void WhenIFillTheRegistrationFormWithInvalidData()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I see validation errors")]
        public void ThenISeeValidationErrors()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I see thank you flash message")]
        public void ThenISeeThankYouFlashMessage()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
