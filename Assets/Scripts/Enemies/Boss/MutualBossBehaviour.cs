
public interface IMutualBossBehaviour
{
    public bool PassedToWait { get; set; }

    public void PlayerStealthCheck();

    public void HandleWaitPhase();
}
