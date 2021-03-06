﻿using Akka.Actor;

namespace Akka.Event
{
    /// <summary>
    ///     Class UnhandledMessage.
    /// </summary>
    public class UnhandledMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnhandledMessage" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="recipient">The recipient.</param>
        internal UnhandledMessage(object message, IActorRef sender, IActorRef recipient)
        {
            Message = message;
            Sender = sender;
            Recipient = recipient;
        }

        /// <summary>
        ///     Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public object Message { get; private set; }

        /// <summary>
        ///     Gets the sender.
        /// </summary>
        /// <value>The sender.</value>
        public IActorRef Sender { get; private set; }

        /// <summary>
        ///     Gets the recipient.
        /// </summary>
        /// <value>The recipient.</value>
        public IActorRef Recipient { get; private set; }
    }
}
