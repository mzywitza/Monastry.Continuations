﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Monastry.Continuations.Tests
{
	[TestFixture]
	public class ContinuationBasicTests
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

		[Test]
		public void ContinuationUsage()
		{
			var o1 = new Order(false, false);
			var o2 = new Order(true, false);
			var o3 = new Order(true, true);

			var v = new OrderValidator();
			var r = new OrderRepo();
			var c = new OrderConfirmationSender();

			var proc = new Continuation<Order>(
				order => order
					.When(o => v.Validate(o))
					.When(o => r.Save(o))
					.Do(o => c.Send(o)));

			proc.Execute(o1);
			proc.Execute(o2);
			proc.Execute(o3);

			Assert.That(c.SentConfirmations.Contains(o1), Is.False);
			Assert.That(c.SentConfirmations.Contains(o2), Is.False);
			Assert.That(c.SentConfirmations.Contains(o3), Is.True);
		}

		[Test]
		public void ContinuationChaining()
		{
			var o1 = new Order(false, false);
			var o2 = new Order(true, false);
			var o3 = new Order(true, true);

			var v = new OrderValidator();
			var r = new OrderRepo();
			var c = new OrderConfirmationSender();

			var validate = new Continuation<Order>(order => order.When(o => v.Validate(o)));
			var save = new Continuation<Order>(order => order.When(o => r.Save(o)));
			var send = new Continuation<Order>(order => order.Do(o => c.Send(o)));
			
			var proc = new Continuation<Order>(
				order => order.When(validate).When(save).Do(send));

			proc.Execute(o1);
			proc.Execute(o2);
			proc.Execute(o3);

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

}
