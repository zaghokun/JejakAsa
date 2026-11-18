#if UNITY_EDITOR
using System.Reflection;
using JejakAsa;
using NUnit.Framework;

public sealed class ObjectiveHudTests
{
    [Test]
    public void ObjectiveFollowsBriefingAndReportProgress()
    {
        var method = typeof(ObjectiveHud).GetMethod("GetObjective", BindingFlags.NonPublic | BindingFlags.Static);
        var progress = new GameProgressData { currentPhase = DayPhase.PoskoBriefing };
        Assert.That(method.Invoke(null, new object[] { progress }), Is.EqualTo("Temui Pak Jonathan"));
        progress.isBriefingDone = true;
        Assert.That(method.Invoke(null, new object[] { progress }), Is.EqualTo("Ambil paket bantuan"));
        progress.hasPickedUpBansos = true;
        Assert.That(method.Invoke(null, new object[] { progress }), Is.EqualTo("Berangkat ke wilayah"));
        progress.currentPhase = DayPhase.PoskoLaporan;
        Assert.That(method.Invoke(null, new object[] { progress }), Is.EqualTo("Laporkan hasil ke Pak Jonathan"));
        progress.isReportDone = true;
        Assert.That(method.Invoke(null, new object[] { progress }), Is.EqualTo("Bicara dengan Ophelia"));
        progress.isOpheliaTalkDone = true;
        Assert.That(method.Invoke(null, new object[] { progress }), Is.EqualTo("Pulang ke rumah"));
    }
}
#endif
