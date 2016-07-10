﻿/*
 Copyright (C) 2008-2013  Andrea Maggiulli (a.maggiulli@gmail.com)

 This file is part of QLNet Project https://github.com/amaggiulli/qlnet

 QLNet is free software: you can redistribute it and/or modify it
 under the terms of the QLNet license.  You should have received a
 copy of the license along with this program; if not, license is
 available online at <http://qlnet.sourceforge.net/License.html>.

 QLNet is a based on QuantLib, a free-software/open-source library
 for financial quantitative analysts and developers - http://quantlib.org/
 The QuantLib license is available online at http://quantlib.org/license.shtml.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace QLNet
{
   public abstract class Claim : IObservable, IObserver
   {
      #region Observer & Observable

      private readonly WeakEventSource eventSource = new WeakEventSource();

      public event Callback notifyObserversEvent
      {
         add { eventSource.Subscribe(value); }
         remove { eventSource.Unsubscribe(value); }
      }

      public void registerWith(Callback handler) { notifyObserversEvent += handler; }

      public void unregisterWith(Callback handler) { notifyObserversEvent -= handler; }

      protected void notifyObservers()
      {
         eventSource.Raise();
      }

      public void update()
      {
         notifyObservers();
      }

      #endregion Observer & Observable

      public abstract double amount(Date defaultDate, double notional, double recoveryRate);
   }

   //! Claim on a notional
   public class FaceValueClaim : Claim
   {
      public override double amount(Date d, double notional, double recoveryRate)
      {
         return notional * (1.0 - recoveryRate);
      }
   }

   //! Claim on the notional of a reference security, including accrual
   public class FaceValueAccrualClaim : Claim
   {
      public FaceValueAccrualClaim(Bond referenceSecurity)
      {
         referenceSecurity_ = referenceSecurity;
         referenceSecurity.registerWith(update);
      }

      public override double amount(Date d, double notional, double recoveryRate)
      {
         double accrual = referenceSecurity_.accruedAmount(d) /
                          referenceSecurity_.notional(d);
         return notional * (1.0 - recoveryRate - accrual);
      }

      private Bond referenceSecurity_;
   }
}
