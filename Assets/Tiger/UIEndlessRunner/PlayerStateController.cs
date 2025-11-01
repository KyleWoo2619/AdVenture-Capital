using UnityEngine;
using UnityEngine.UI;

public class PlayerStateController : MonoBehaviour
{
    public enum PlayerState
    {
        Default,
        Duos,
        Quad
    }

    [Header("Sprites per State")]
    public Sprite defaultSprite;
    public Sprite duosSprite;
    public Sprite quadSprite;

    private Image image;
    private PlayerState currentState = PlayerState.Default;

    void Awake()
    {
        image = GetComponent<Image>();
        SetState(PlayerState.Default);
    }

    public void SetState(PlayerState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case PlayerState.Default:
                image.sprite = defaultSprite;
                break;
            case PlayerState.Duos:
                image.sprite = duosSprite;
                break;
            case PlayerState.Quad:
                image.sprite = quadSprite;
                break;
        }
    }

    public void LevelUpState()
    {
        int next = (int)currentState + 1;
        if (next >= System.Enum.GetValues(typeof(PlayerState)).Length)
        {
            next = (int)currentState; // cap at highest state
        }

        SetState((PlayerState)next);
    }

    public PlayerState GetState() => currentState;
}
