using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace LinqContinuations
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void Test()
        {
            var o1 = new Order(false, false);
            var o2 = new Order(true, false);
            var o3 = new Order(true, true);

            var v = new OrderValidator();
            var r = new OrderRepo();
            var c = new OrderConfirmationSender();

            var proc = new OrderProcessor(v, r, c);

            proc.Process(o1);
            proc.Process(o2);
            proc.Process(o3);

            Assert.That(c.SentConfirmations.Contains(o1), Is.False);
            Assert.That(c.SentConfirmations.Contains(o2), Is.False);
            Assert.That(c.SentConfirmations.Contains(o3), Is.True);
        }
    }

    public class OrderProcessor
    {
        private OrderValidator _validator;
        private OrderRepo _repo;
        private OrderConfirmationSender _sender;

        public OrderProcessor(OrderValidator validator, OrderRepo repo, OrderConfirmationSender sender)
        {
            _validator = validator;
            _repo = repo;
            _sender = sender;
        }

        public void Process(Order order)
        {
            order
                .When(o => _validator.Validate(o))
                .When(o => _repo.Save(o))
                .Do(o => _sender.Send(o));
        }
    }

    public class Order
    {
        private bool _isValid;
        private bool _isSavable;

        public Order(bool isValid, bool isSavable)
        {
            _isValid = isValid;
            _isSavable = isSavable;
        }

        public bool IsValid
        {
            get { return _isValid; }
        }

        public bool IsSavable
        {
            get { return _isSavable; }
        }
    }

    public class OrderValidator
    {
        public bool Validate(Order order)
        {
            return order.IsValid;
        }
    }

    public class OrderRepo
    {
        public bool Save(Order order)
        {
            return order.IsSavable;
        }
    }

    public class OrderConfirmationSender
    {
        public readonly IList<Order> SentConfirmations = new List<Order>();

        public void Send(Order order)
        {
            SentConfirmations.Add(order);
        }
    }

    public static class ObjectExtensions
    {
        public static T When<T>(this T subject, Func<T, bool> tester) where T:class
        {
            return (subject != null && tester(subject)) ? subject : null;
        }

        public static T Do<T>(this T subject, Action<T> worker) where T : class
        {
            if (subject == null) return null;
            worker(subject);
            return subject;
        }

    }
}
