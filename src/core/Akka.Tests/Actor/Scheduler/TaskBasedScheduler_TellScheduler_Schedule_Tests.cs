using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.TestKit;
using Xunit;
using Xunit.Extensions;

namespace Akka.Tests.Actor.Scheduler
{
    // ReSharper disable once InconsistentNaming
    public class TaskBasedScheduler_TellScheduler_Schedule_Tests : AkkaSpec
    {
        [Theory(Skip = "Tests that messages are sent with the specified interval, however due to inaccuracy of Task.Dely this often fails. Run this manually if you've made changes to TaskBasedScheduler")]
        [InlineData(10, 1000)]
        public void ScheduleTellRepeatedly_in_milliseconds_Tests(int initialDelay, int interval)
        {
            // Prepare, set up actions to be fired
            IScheduler scheduler = new TaskBasedScheduler();

            var cancelable = new Cancelable(Sys.Scheduler);
            var receiver = ActorOf(dsl =>
            {
                //Receive three messages, and store the time when these were received
                //after three messages stop the actor and send the times to TestActor
                var messages = new List<DateTimeOffset>();
                dsl.Receive<string>((s, context) =>
                {
                    messages.Add(context.System.Scheduler.Now);
                    if(messages.Count == 3)
                    {
                        TestActor.Tell(messages);
                        cancelable.Cancel();
                        context.Stop(context.Self);
                    }
                });
            });
            scheduler.ScheduleTellRepeatedly(initialDelay, interval, receiver, "Test", ActorRefs.NoSender, cancelable);

            //Expect to get a list from receiver after it has received three messages
            var dateTimeOffsets = ExpectMsg<List<DateTimeOffset>>();
            dateTimeOffsets.ShouldHaveCount(3);
            Action<int, int> validate = (a, b) =>
            {
                var valA = dateTimeOffsets[a];
                var valB = dateTimeOffsets[b];
                var diffBetweenMessages = Math.Abs((valB - valA).TotalMilliseconds);
                var diffInMs = Math.Abs(diffBetweenMessages - interval);
                var deviate = (diffInMs / interval);
                deviate.Should(val => val < 0.1, string.Format("Expected the interval between message {1} and {2} to deviate maximum 10% from {0}. It was {3} ms between the messages. It deviated {4}%", interval, a + 1, b + 1, diffBetweenMessages, deviate * 100));
            };
            validate(0, 1);
            validate(1, 2);
        }



        [Theory]
        [InlineData(10, 50)]
        [InlineData(00, 50)]
        public void ScheduleTellRepeatedly_TimeSpan_Tests(int initialDelay, int interval)
        {
            //Prepare, set up actions to be fired
            IScheduler scheduler = new TaskBasedScheduler();

            scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(initialDelay), TimeSpan.FromMilliseconds(interval), TestActor, "Test", ActorRefs.NoSender);

            //Just check that we receives more than one message
            ExpectMsg("Test");
            ExpectMsg("Test");
            ExpectMsg("Test");
        }


        [Theory]
        [InlineData(new int[] { 1, 50, 110 })]
        public void ScheduleTellOnceTests(int[] times)
        {
            // Prepare, set up messages to be sent
            IScheduler scheduler = new TaskBasedScheduler();

            foreach(var time in times)
            {
                scheduler.ScheduleTellOnce(time, TestActor, "Test" + time, ActorRefs.NoSender);
            }

            ExpectMsg("Test1");
            ExpectMsg("Test50");
            ExpectMsg("Test110");

            ExpectNoMsg(50);
        }


        [Theory]
        [InlineData(new int[] { 1, 1, 50, 50, 100, 100 })]
        public void When_ScheduleTellOnce_many_at_the_same_time_Then_all_fires(int[] times)
        {
            // Prepare, set up actions to be fired
            IScheduler scheduler = new TaskBasedScheduler();

            foreach(var time in times)
            {
                scheduler.ScheduleTellOnce(time, TestActor, "Test" + time, ActorRefs.NoSender);
            }

            //Perform the test
            ExpectMsg("Test1");
            ExpectMsg("Test1");
            ExpectMsg("Test50");
            ExpectMsg("Test50");
            ExpectMsg("Test100");
            ExpectMsg("Test100");
            ExpectNoMsg(50);
        }


        [Theory]
        [InlineData(-1)]
        [InlineData(-4711)]
        public void When_ScheduleTellOnce_with_invalid_delay_Then_exception_is_thrown(int invalidTime)
        {
            IScheduler scheduler = new TaskBasedScheduler();

            XAssert.Throws<ArgumentOutOfRangeException>(() =>
                scheduler.ScheduleTellOnce(invalidTime, TestActor, "Test", ActorRefs.NoSender)
                );
            ExpectNoMsg(50);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-4711)]
        public void When_ScheduleTellRepeatedly_with_invalid_delay_Then_exception_is_thrown(int invalidTime)
        {
            IScheduler scheduler = new TaskBasedScheduler();

            XAssert.Throws<ArgumentOutOfRangeException>(() =>
                scheduler.ScheduleTellRepeatedly(invalidTime, 100, TestActor, "Test", ActorRefs.NoSender)
                );
            ExpectNoMsg(50);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-4711)]
        public void When_ScheduleTellRepeatedly_with_invalid_interval_Then_exception_is_thrown(int invalidInterval)
        {
            IScheduler scheduler = new TaskBasedScheduler();

            XAssert.Throws<ArgumentOutOfRangeException>(() =>
                scheduler.ScheduleTellRepeatedly(42, invalidInterval, TestActor, "Test", ActorRefs.NoSender)
                );
            ExpectNoMsg(50);
        }

        [Fact]
        public void When_ScheduleTellOnce_with_0_delay_Then_action_is_executed_immediately()
        {
            IScheduler scheduler = new TaskBasedScheduler();
            scheduler.ScheduleTellOnce(0, TestActor, "Test", ActorRefs.NoSender);
            ExpectMsg("Test");
        }

        [Fact]
        public void When_ScheduleTellRepeatedly_with_0_delay_Then_action_is_executed_immediately()
        {
            IScheduler scheduler = new TaskBasedScheduler();
            scheduler.ScheduleTellRepeatedly(0, 60 * 1000, TestActor, "Test", ActorRefs.NoSender);
            ExpectMsg("Test");
        }
    }
}