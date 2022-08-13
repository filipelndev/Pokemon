using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] FacingDiretion defaultDirection = FacingDiretion.Down;

   //Parametros
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }

   //Animações
   SpriteAnimator walkDownAnim;
   SpriteAnimator walkUpAnim;
   SpriteAnimator walkLeftAnim;
   SpriteAnimator walkRightAnim;

    //Animação atual
   SpriteAnimator currentAnim;
   bool wasPreviouslyMoving;

   //Referenciar
   SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        SetFacingDirection(defaultDirection);
        currentAnim= walkDownAnim;
    }
    private void Update()
    {
        var prevAnim = currentAnim;

        if(MoveX == 1)
            currentAnim = walkRightAnim;
        else if(MoveX == -1)
            currentAnim = walkLeftAnim;
        else if(MoveY == 1)
            currentAnim = walkUpAnim;
        else if(MoveY == -1)
            currentAnim = walkDownAnim;

        if(currentAnim != prevAnim || IsMoving != wasPreviouslyMoving)
            currentAnim.Start();

        if(IsMoving)
            currentAnim.HandleUpdate();
        else
            spriteRenderer.sprite = currentAnim.Frames[0];
            wasPreviouslyMoving = IsMoving;
    }

    public void SetFacingDirection(FacingDiretion dir)
    {
        if(dir == FacingDiretion.Right)
            MoveX = 1;
        else if(dir == FacingDiretion.Left)
            MoveX = -1;
        else if(dir == FacingDiretion.Up)
            MoveY = 1;
        else if(dir == FacingDiretion.Down)
            MoveY = -1;
    }
    public FacingDiretion DefaultDirection{
        get => defaultDirection;
    }
}

public enum FacingDiretion { Up, Down, Left, Right }
