using System;
using Moq;
using NUnit.Framework;

namespace MoqTest
{
  [TestFixture]
  public class MoqTest
  {
    [Test]
    public void Setup_ExtraCall_Strict ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Strict);
      myMock.Setup (x => x.DoTheThing ("A")).Returns ("1").Verifiable();
      myMock.Setup (x => x.DoTheThing ("B")).Returns ("2").Verifiable();

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (myMock.Object.DoTheThing ("B"), Is.EqualTo ("2"));
      Assert.That (
          () => myMock.Object.DoTheThing ("C"),
          Throws.TypeOf<MockException>().With.Message.EqualTo (
              "IMyInterface.DoTheThing(\"C\") invocation failed with mock behavior Strict.\nAll invocations on the mock must have a corresponding setup."));

      myMock.Verify();
      // Strict mocks do not fail in Verify() if not all calls have been set up. They fail at the call site.

      Assert.That (
          () => myMock.VerifyNoOtherCalls(),
          Throws.TypeOf<MockException>().With.Message.StartWith (
              "Mock<IMyInterface:").And.Message.EndWith (">:\n"
              + "This mock failed verification due to the following unverified invocations:\r\n"
              + "\r\n"
              + "   IMyInterface.DoTheThing(\"C\")"));
    }

    [Test]
    public void Setup_ExtraCall_Loose ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      myMock.Setup (x => x.DoTheThing ("A")).Returns ("1").Verifiable();
      myMock.Setup (x => x.DoTheThing ("B")).Returns ("2").Verifiable();

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (myMock.Object.DoTheThing ("B"), Is.EqualTo ("2"));
      Assert.That (() => myMock.Object.DoTheThing ("C"), Is.Null);

      myMock.Verify();
      // Loose mocks do not fail in Verify() or at the call site if not all calls have been set up.

      Assert.That (
          () => myMock.VerifyNoOtherCalls(),
          Throws.TypeOf<MockException>().With.Message.StartWith (
              "Mock<IMyInterface:").And.Message.EndWith (">:\n"
              + "This mock failed verification due to the following unverified invocations:\r\n"
              + "\r\n"
              + "   IMyInterface.DoTheThing(\"C\")"));
    }

    [Test]
    public void Setup_NotAllSetupsAreUsed_Strict ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Strict);
      myMock.Setup (x => x.DoTheThing ("A")).Returns ("1").Verifiable();
      myMock.Setup (x => x.DoTheThing ("B")).Returns ("2").Verifiable();
      myMock.Setup (x => x.DoTheThing ("C")).Returns ("3").Verifiable();

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (myMock.Object.DoTheThing ("C"), Is.EqualTo ("3"));

      Assert.That (
          () => myMock.Verify(),
          Throws.TypeOf<MockException>().With.Message.StartWith (
              "Mock<IMyInterface:").And.Message.EndWith (">:\n"
              + "This mock failed verification due to the following:\r\n"
              + "\r\n"
              + "   IMyInterface x => x.DoTheThing(\"B\"):\n"
              + "   This setup was not matched."));
    }

    [Test]
    public void Setup_NotAllSetupsAreUsed_Loose ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      myMock.Setup (x => x.DoTheThing ("A")).Returns ("1").Verifiable();
      myMock.Setup (x => x.DoTheThing ("B")).Returns ("2").Verifiable();
      myMock.Setup (x => x.DoTheThing ("C")).Returns ("3").Verifiable();

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (myMock.Object.DoTheThing ("C"), Is.EqualTo ("3"));

      Assert.That (
          () => myMock.Verify(),
          Throws.TypeOf<MockException>().With.Message.StartWith (
              "Mock<IMyInterface:").And.Message.EndWith (">:\n"
              + "This mock failed verification due to the following:\r\n"
              + "\r\n"
              + "   IMyInterface x => x.DoTheThing(\"B\"):\n"
              + "   This setup was not matched."));
    }

    [Test]
    public void Setup_OverrideSameParameters_Strict ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Strict);
      myMock.Setup (x => x.DoTheThing ("A")).Returns ("1").Verifiable();
      myMock.Setup (x => x.DoTheThing ("A")).Returns ("2").Verifiable();

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("2"));
      // Second setup overwrites first setup

      myMock.Verify();
      // Strict mocks do not fail verification if not all setups have been used.
    }

    [Test]
    public void InSequence ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Strict);
      myMock.SetupSequence (x => x.DoTheThing ("A")).Returns ("1").Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("2"));
    }

    [Test]
    public void InSequence_LastSetupNotUsed ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Strict);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("3");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("2"));

      myMock.Verify();
      // Strict mocks do not fail verification if not all setups have been used.

      Assert.That (
          () => myMock.VerifyNoOtherCalls(),
          Throws.TypeOf<MockException>().With.Message.StartWith (
              "Mock<IMyInterface:").And.Message.EndWith (">:\n"
              + "This mock failed verification due to the following unverified invocations:\r\n"
              + "\r\n"
              + "   IMyInterface.DoTheThing(\"A\")\r\n"
              + "   IMyInterface.DoTheThing(\"A\")"));
    }

    [Test]
    public void InSequence_UnexpectedCall_Strict ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Strict);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (
          () => myMock.Object.DoTheThing ("B"),
          Throws.TypeOf<MockException> ().With.Message.EqualTo (
              "IMyInterface.DoTheThing(\"B\") invocation failed with mock behavior Strict.\nAll invocations on the mock must have a corresponding setup."));
      // Unknown call, DoTheThing("B") is not matched.
    }

    [Test]
    public void InSequence_UnexpectedCall_Loose ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (myMock.Object.DoTheThing ("B"), Is.Null);
      // Unknown call, DoTheThing("B") is not matched.

      myMock.Verify();
      // Loose mocks do not fail verification if extra calls are made
    }

    [Test]
    public void InSequence_UnorderedCall_Strict ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Strict);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("B")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (
          () => myMock.Object.DoTheThing ("A"),
          Throws.TypeOf<MockException> ().With.Message.EqualTo (
              "IMyInterface.DoTheThing(\"A\") invocation failed with mock behavior Strict.\nAll invocations on the mock must have a corresponding setup."));
      // Ordered expectation is not satisfied, DoTheThing("A") is not matched.
    }

    [Test]
    public void InSequence_UnorderedCall_Loose ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("B")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.Null);
      // Ordered expectation is not satisfied, DoTheThing("A") is not matched.

      myMock.Verify();
      // Loose mocks do not fail verification if extra calls are made, in this case a call that is not done in the proper order
    }
  }
}