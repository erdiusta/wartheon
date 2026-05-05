using UnityEngine;

public class AnimateSkillManager : MonoBehaviour
{
    Player player;

    public Animator firstSkillAnimator;
    public Animator secondSkillAnimator;
    public Animator thirdSkillAnimator;
    public Animator fourthSkillAnimator;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    public void ApplyValor(bool undo)
    {
        firstSkillAnimator.SetBool("valor", !undo);
    }

    public void ApplyGuardedOath(bool undo)
    {
        secondSkillAnimator.SetBool("oath", !undo);
    }

    public void ApplyShadowStep(bool undo)
    {
        thirdSkillAnimator.SetBool("shadowStep", !undo);
    }

    public void ApplyPenetrate()
    {
        secondSkillAnimator.SetTrigger("penetrate");
    }

    public void ApplyTripleThreat()
    {
        thirdSkillAnimator.SetTrigger("tripleThreat");
    }

    public void ApplyMycarasSeal(bool undo)
    {
        if (!undo)
        {
            secondSkillAnimator.gameObject.SetActive(true);
            secondSkillAnimator.SetBool("mycarasSeal", true);
        }
        else
        {
            secondSkillAnimator.SetBool("mycarasSeal", false);
            secondSkillAnimator.gameObject.SetActive(false);
        }
    }

    public void ApplyKynarasEmbrace(bool undo)
    {
        if (!undo)
        {
            secondSkillAnimator.gameObject.SetActive(true);
            secondSkillAnimator.SetBool("kynarasEmbrace", true);
        }
        else
        {
            secondSkillAnimator.SetBool("kynarasEmbrace", false);
            secondSkillAnimator.gameObject.SetActive(false);
        }
    }

    public void ApplyVenomousIvy(bool undo)
    {
        if (!undo)
        {
            secondSkillAnimator.gameObject.SetActive(true);
            secondSkillAnimator.SetBool("venomousIvy", true);
        }
        else
        {
            secondSkillAnimator.SetBool("venomousIvy", false);
            secondSkillAnimator.gameObject.SetActive(false);
        }
    }

    public void ApplyFadeAndFeed(bool undo)
    {
        if (!undo)
        {
            thirdSkillAnimator.gameObject.SetActive(true);
            thirdSkillAnimator.SetBool("fadeAndFeed", true);
        }
        else
        {
            thirdSkillAnimator.SetBool("fadeAndFeed", false);
            thirdSkillAnimator.gameObject.SetActive(false);
        }
    }

    public void ApplyBladeDash(bool undo)
    {
        if (!undo)
        {
            fourthSkillAnimator.gameObject.SetActive(true);
            fourthSkillAnimator.SetBool("bladeDash", true);
        }
        else
        {
            fourthSkillAnimator.SetBool("bladeDash", false);
            fourthSkillAnimator.gameObject.SetActive(false);
        }
    }

    public void ApplyNymarasWindveil(bool undo)
    {
        if (!undo)
        {
            secondSkillAnimator.gameObject.SetActive(true);
            secondSkillAnimator.SetBool("nymarasWindveil", true);
        }
        else
        {
            secondSkillAnimator.SetBool("nymarasWindveil", false);
            secondSkillAnimator.gameObject.SetActive(false);
        }
    }

    public void ApplyIonicRejuvenation(bool undo)
    {
        if (!undo)
        {
            thirdSkillAnimator.gameObject.SetActive(true);
            thirdSkillAnimator.SetBool("ionicRejuvenation", true);
        }
        else
        {
            thirdSkillAnimator.SetBool("ionicRejuvenation", false);
            thirdSkillAnimator.gameObject.SetActive(false);
        }
    }
}
