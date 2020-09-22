using System;
using Moq;
using NUnit.Framework;

namespace MoqTest
{
  [TestFixture]
  public class MoqTest
  {
    // Unordered extra call strict fails during invocation.
    // Unordered extra call loose fails during VerifyNoOtherCalls but not during Verify.
    // Unordered missing call strict or loose fails during Verify.

    // Ordered extra call strict fails during invocation.
    // Ordered extra call loose fails during VerifyNoOtherCalls but not during Verify. Subsequent calls are not matched.
    // Ordered missing call strict or loose fails during VerifyNoOtherCalls but not during Verify.

    // extra call = call that invoked but not set up
    // missing call = call that was set up but not invoked

    //TODO: Expression-Arguments (ItExpr vs It)

    [Test]
    public void Setup_Verifiable_ExtraCall_Strict_FailsDuringInvocation ()
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
    }

    [Test]
    public void Setup_Verifiable_ExtraCall_Loose_DoesNotFailsDuringVerify()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      myMock.Setup (x => x.DoTheThing ("A")).Returns ("1").Verifiable();
      myMock.Setup (x => x.DoTheThing ("B")).Returns ("2").Verifiable();

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (myMock.Object.DoTheThing ("B"), Is.EqualTo ("2"));
      Assert.That (() => myMock.Object.DoTheThing ("C"), Is.Null);

      Assert.That (() => myMock.Verify(), Throws.Nothing);
    }

    [Test]
    public void Setup_Verifiable_ExtraCall_Loose_FailsDuringVerifyNoOtherCalls ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      myMock.Setup (x => x.DoTheThing ("A")).Returns ("1").Verifiable();
      myMock.Setup (x => x.DoTheThing ("B")).Returns ("2").Verifiable();

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (myMock.Object.DoTheThing ("B"), Is.EqualTo ("2"));
      Assert.That (() => myMock.Object.DoTheThing ("C"), Is.Null);

      myMock.Verify();
      // Verify is required to clean-up the known matches before calling VerifyNoOtherCalls

      Assert.That (
          () => myMock.VerifyNoOtherCalls(),
          Throws.TypeOf<MockException>().With.Message.StartWith (
              "Mock<IMyInterface:").And.Message.EndWith (">:\n"
              + "This mock failed verification due to the following unverified invocations:\r\n"
              + "\r\n"
              + "   IMyInterface.DoTheThing(\"C\")"));
    }

    [Test]
    public void Setup_Verifiable_NotAllSetupsAreUsed_Strict_FailsDuringVerify ()
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
    public void Setup_Verifiable_NotAllSetupsAreUsed_Loose_FailsDuringVerify ()
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
    public void Setup_Verifiable_OverrideSameParameters_Strict_DoesNotFail ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Strict);
      myMock.Setup (x => x.DoTheThing ("A")).Returns ("1").Verifiable();
      myMock.Setup (x => x.DoTheThing ("A")).Returns ("2").Verifiable();

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("2"));
      // Second setup overwrites first setup

      Assert.That (() => myMock.Verify(), Throws.Nothing);
    }

    [Test]
    public void SetupSequence ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Strict);
      myMock.SetupSequence (x => x.DoTheThing ("A")).Returns ("1").Returns ("2");

      // Act
      myMock.Object.DoTheThing ("A");

      Assert.That (() => myMock.Verify(), Throws.Nothing);
    }

    [Test]
    public void Setup_InSequence_NotAllSetupAreUsed_Strict_DoesNotFailDuringVerify ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Strict);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));

      Assert.That (() => myMock.Verify(), Throws.Nothing);

      //TODO: This seems like an inconsistency
    }

    [Test]
    public void Setup_InSequence_NotAllSetupAreUsed_Strict_FailsDuringVerifyNoOtherCalls ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Strict);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));

      myMock.Verify();
      // Verify does not affect VerifyNoOtherCalls

      Assert.That (
          () => myMock.VerifyNoOtherCalls(),
          Throws.TypeOf<MockException>().With.Message.StartWith (
              "Mock<IMyInterface:").And.Message.EndWith (">:\n"
              + "This mock failed verification due to the following unverified invocations:\r\n"
              + "\r\n"
              + "   IMyInterface.DoTheThing(\"A\")"));
    }

    [Test]
    public void Setup_InSequence_NotAllSetupAreUsed_Loose_DoesNotFailDuringVerify ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));

      Assert.That (() => myMock.Verify(), Throws.Nothing);
    }

    [Test]
    public void Setup_InSequence_NotAllSetupAreUsed_Loose_FailsDuringVerifyNoOtherCalls ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));

      myMock.Verify();
      // Verify does not affect VerifyNoOtherCalls

      Assert.That (
          () => myMock.VerifyNoOtherCalls(),
          Throws.TypeOf<MockException>().With.Message.StartWith (
              "Mock<IMyInterface:").And.Message.EndWith (">:\n"
              + "This mock failed verification due to the following unverified invocations:\r\n"
              + "\r\n"
              + "   IMyInterface.DoTheThing(\"A\")"));
    }

    [Test]
    public void Setup_InSequence_UnexpectedCall_Strict_FailsDuringInvocation ()
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
    public void Setup_InSequence_UnexpectedCall_Loose_DoesNotFailDuringVerify ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (myMock.Object.DoTheThing ("B"), Is.Null);
      // Unknown call, DoTheThing("B") is not matched.

      Assert.That (() => myMock.Verify(), Throws.Nothing);
    }

    [Test]
    public void Setup_InSequence_UnexpectedCall_Loose_DoesNotMatch_FailsDuringVerifyNoOtherCalls ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.EqualTo ("1"));
      Assert.That (myMock.Object.DoTheThing ("B"), Is.Null);
      // Unknown call, DoTheThing("B") is not matched.

      Assert.That (
          () => myMock.VerifyNoOtherCalls(),
          Throws.TypeOf<MockException>().With.Message.StartWith (
              "Mock<IMyInterface:").And.Message.EndWith (">:\n"
              + "This mock failed verification due to the following unverified invocations:\r\n"
              + "\r\n"
              + "   IMyInterface.DoTheThing(\"A\")\r\n"
              + "   IMyInterface.DoTheThing(\"B\")"));
    }

    [Test]
    public void Setup_InSequence_UnorderedCall_Strict_FailsDuringInvocation ()
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
    public void Setup_InSequence_UnorderedCall_Loose_DoesNotMatch_DoesNotFailDuringVerify ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("B")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.Null);
      // Ordered expectation is not satisfied, DoTheThing("A") is not matched.

      Assert.That (() => myMock.Verify(), Throws.Nothing);
    }

    [Test]
    public void Setup_InSequence_UnorderedCall_Loose_DoesNotMatch_FailsDuringVerifyNoOtherCalls ()
    {
      var myMock = new Mock<IMyInterface> (MockBehavior.Loose);
      var sequence = new MockSequence();
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("B")).Returns ("1");
      myMock.InSequence (sequence).Setup (x => x.DoTheThing ("A")).Returns ("2");

      // Act
      Assert.That (myMock.Object.DoTheThing ("A"), Is.Null);
      // Ordered expectation is not satisfied, DoTheThing("A") is not matched.

      Assert.That (
          () => myMock.VerifyNoOtherCalls(),
          Throws.TypeOf<MockException>().With.Message.StartWith (
              "Mock<IMyInterface:").And.Message.EndWith (">:\n"
              + "This mock failed verification due to the following unverified invocations:\r\n"
              + "\r\n"
              + "   IMyInterface.DoTheThing(\"A\")"));
    }
  }
}