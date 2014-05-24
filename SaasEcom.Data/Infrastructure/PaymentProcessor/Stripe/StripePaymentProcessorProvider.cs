﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SaasEcom.Data.Models;
using Stripe;

namespace SaasEcom.Data.Infrastructure.PaymentProcessor.Stripe
{
    // TODO: Split this class and eventually completely remove it.
    public class StripePaymentProcessorProvider
    {
        private readonly string _apiKey;

        private StripeCustomerService _customerService;
        private StripeCustomerService CustomerService
        {
            get { return _customerService ?? (_customerService = new StripeCustomerService(_apiKey)); }
        }

        private StripePlanService _planService;
        private StripePlanService PlanService
        {
            get { return _planService ?? (_planService = new StripePlanService(_apiKey)); }
        }

        private StripeInvoiceService _invoiceService;
        private StripeInvoiceService InvoiceService
        {
            get { return _invoiceService ?? (_invoiceService = new StripeInvoiceService(_apiKey)); }
        }

        public StripePaymentProcessorProvider(string apiKey)
        {
            _apiKey = apiKey;
        }

        #region Subscription Plans

        public StripePlan CreateSubscriptionPlan(SubscriptionPlan plan)
        {
            var result = PlanService.Create(new StripePlanCreateOptions
            {
                Id = plan.FriendlyId,
                Name = plan.Name,
                Amount = (int) Math.Round(plan.Price * 100),
                Currency = plan.Currency,
                Interval = GetInterval(plan.Interval),
                TrialPeriodDays = plan.TrialPeriodInDays,
                IntervalCount = 1, // The number of intervals (specified in the interval property) between each subscription billing. For example, interval=month and interval_count=3 bills every 3 months.
            });

            return result;
        }
        
        public StripePlan UpdateSubscriptionPlan(SubscriptionPlan plan)
        {
            var res = PlanService.Update(plan.FriendlyId, new StripePlanUpdateOptions
            {
                Name = plan.Name
            });

            return res;
        }

        public StripePlan GetSubscriptionPlan(string planId)
        {
            try
            {
                return PlanService.Get(planId);
            }
            catch (StripeException ex)
            {
                return null;
            }
        }

        public IEnumerable<StripePlan> GetAllPlans(StripeListOptions options)
        {
            return PlanService.List(options);
        }

        public void DeleteSubscriptionPlan(string planId)
        {
            PlanService.Delete(planId);
        }

        #endregion

        #region Customers

        public async Task<StripeCustomer> CreateCustomerAsync(ApplicationUser user, string planId = null)
        {
            var customer = new StripeCustomerCreateOptions
            {
                AccountBalance = 0,
                // Card
                Email = user.Email,
            };

            if (!string.IsNullOrEmpty(planId))
            {
                customer.PlanId = planId;
            }

            return await Task.Run(() => CustomerService.Create(customer));
        }

        public StripeCustomer UpdateCustomer(ApplicationUser user, CreditCard card)
        {
            var customer = new StripeCustomerUpdateOptions
            {
                Email = user.Email,
                
                // Card Details
                CardAddressCity = card.AddressCity,
                CardAddressCountry = card.AddressCountry,
                CardAddressLine1 = card.AddressLine1,
                CardAddressLine2 = card.AddressLine2,
                CardAddressState = card.AddressState,
                CardAddressZip = card.AddressZip,
                CardCvc = card.Cvc,
                CardExpirationMonth = card.ExpirationMonth,
                CardExpirationYear = card.ExpirationYear,
                CardName = card.Name,
                CardNumber = card.CardNumber
            };

            return CustomerService.Update(user.StripeCustomerId, customer);
        }

        public StripeCustomer GetCustomer(string customerId)
        {
            return CustomerService.Get(customerId);
        }

        public void DeleteCustomer(string customerId)
        {
            CustomerService.Delete(customerId);
        }

        public IEnumerable<StripeCustomer> GetAllCustomers(int limit = 100)
        {
            return CustomerService.List(new StripeCustomerListOptions() { Limit = limit });
        }

        #endregion

        #region Subscriptions

        public StripeSubscription UpdateCustomerSubscription(string customerId, CreditCard creditCard, string planId)
        {
            var myUpdatedSubscription = new StripeCustomerUpdateSubscriptionOptions
            {
                CardNumber = creditCard.CardNumber,
                CardExpirationYear = creditCard.ExpirationYear,
                CardExpirationMonth = creditCard.ExpirationMonth,
                CardAddressCountry = creditCard.AddressCountry,
                CardAddressLine1 = creditCard.AddressLine1,
                CardAddressLine2 = creditCard.AddressLine2,
                CardAddressState = creditCard.AddressState,
                CardAddressZip = creditCard.AddressZip,
                CardName = creditCard.Name,
                CardCvc = creditCard.Cvc,
                CardAddressCity = creditCard.AddressCity,
                PlanId = planId
            };

            return CustomerService.UpdateSubscription(customerId, myUpdatedSubscription);
        }

        public StripeSubscription CancelCustomerSubscription(string customerId, string subscriptionId, bool cancelAtPeriodEnd = false)
        {
            // TODO Pass also subscription Id
            return CustomerService.CancelSubscription(customerId/*, subscriptionId*/, cancelAtPeriodEnd);
        }

        #endregion

        #region Helpers
        
        private static string GetInterval(SubscriptionPlan.SubscriptionInterval interval)
        {
            string result = null;

            switch (interval)
            {
                case (SubscriptionPlan.SubscriptionInterval.Monthly):
                    result = "month";
                    break;
                case (SubscriptionPlan.SubscriptionInterval.Yearly):
                    result = "year";
                    break;
                case (SubscriptionPlan.SubscriptionInterval.Weekly):
                    result = "week";
                    break;
                case (SubscriptionPlan.SubscriptionInterval.EveryThreeMonths):
                    result = "3-month";
                    break;
                case (SubscriptionPlan.SubscriptionInterval.EverySixMonths):
                    result = "6-month";
                    break;
            }

            return result;
        }

        #endregion
    }
}
