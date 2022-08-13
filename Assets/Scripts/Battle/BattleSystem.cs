using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using DG.Tweening;


public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, MoveToForget, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run };
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveScriptUI moveSelectionUI;

   public event Action<bool> onBattleOver;

   BattleState state;
   

   int currentAction;
   int currentMove;

   int escapeAttemps;
   MoveBase moveToLearn;
   bool aboutToUseChoice = true;

   PokemonParty playerParty;
   PokemonParty trainerParty;
   Pokemon wildPokemon;

   bool isTrainerBattle = false;
   PlayerController player;
   TrainerController trainer;

   public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon) 
   {
       this.playerParty = playerParty;
       this.wildPokemon = wildPokemon;
       player = playerParty.GetComponent<PlayerController>();
       isTrainerBattle = false;

       StartCoroutine(SetupBattle());
   }

    public void StartTrainerBattle(PokemonParty playerParty,PokemonParty trainerParty) 
   {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
   }

   public IEnumerator SetupBattle(){

        playerUnit.Clear();
        enemyUnit.Clear();
   
       if(!isTrainerBattle)
       {
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared.");
       }
       else
       {
           //Mostrar os treinadores antes dos pokemons entrar
           playerUnit.gameObject.SetActive(false);
           enemyUnit.gameObject.SetActive(false);

           playerImage.gameObject.SetActive(true);
           trainerImage.gameObject.SetActive(true);

           playerImage.sprite = player.Sprite;
           trainerImage.sprite = trainer.Sprite;

           yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");
           
           //Primeiro pokemon do treinador adversário
           trainerImage.gameObject.SetActive(false);
           enemyUnit.gameObject.SetActive(true);
           var enemyPokemon = trainerParty.GetHealthyPokemon();
           enemyUnit.Setup(enemyPokemon);
           yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyPokemon.Base.Name}");

            //Primeiro pokemon do player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
       }

       escapeAttemps = 0;

       partyScreen.Init();
       ActionSelection();
   }



   void BattleOver(bool won) 
   {
       state = BattleState.BattleOver;
       playerParty.Pokemons.ForEach(p => p.OnBattleOver());
       onBattleOver(won);
   }

   void ActionSelection()
   {
       state= BattleState.ActionSelection;
       dialogBox.SetDialog("Choose an action:");
       dialogBox.EnableActionSelector(true);
   }

   void MoveSelection()
   {
       state = BattleState.MoveSelection;
       dialogBox.EnableActionSelector(false);
       dialogBox.EnableDialogText(false);
       dialogBox.EnableMoveSelector(true);
   }

   IEnumerator AboutToUse(Pokemon newPokemon)
   {
       state = BattleState.Busy;
       yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newPokemon.Base.Name}. Do you want to change pokemon?");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
   }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you wan't to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }   

    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if(playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            //Verifica quem é o primeiro a agir
            bool playerGoesFirst = true;
            if(enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            //Primeiro turno
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if(state == BattleState.BattleOver) yield break;
            
            if(secondPokemon.HP > 0)
            {
            //Próximo turno
            yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(secondUnit);
            if(state == BattleState.BattleOver) yield break;
            }
        }
        else
        {   //Troca pokemon
            if(playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.selectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if(playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowPokeball();
            }
            else if(playerAction == BattleAction.Run)
            {
                dialogBox.EnableActionSelector(false);
                yield return TryToEscape();
            }

            //Turno do inimigo
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if(state == BattleState.BattleOver) yield break;
            
        }

        if(state != BattleState.BattleOver)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        
       bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
       if(!canRunMove)
       {
           yield return ShowStatusChange(sourceUnit.Pokemon);
           yield return sourceUnit.Hud.UpdateHP();
           yield break;
       }
        yield return ShowStatusChange(sourceUnit.Pokemon);

       move.PP--;
       yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

       if(CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if(move.Base.Category == MoveCategory.Status)
                {
                    yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
                }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if(rnd <= secondary.Chance)
                    yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                }
            }
            if(targetUnit.Pokemon.HP <= 0)
            {
               yield return HandlePokemonFainted(targetUnit);
            }
        }
       else
       {
           yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed");
       }
        
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        //Buff Status
           if(effects.Boosts != null)
           {
               if(moveTarget == MoveTarget.Self)
                    source.ApplyBoots(effects.Boosts);
                else    
                    target.ApplyBoots(effects.Boosts);
           }
            //Status
           if(effects.Status != ConditionId.none)
           {
               target.SetStatus(effects.Status);
           }
            //VolatileStatus
           if(effects.VolatileStatus != ConditionId.none)
           {
               target.SetVolatileStatus(effects.VolatileStatus);
           }

           yield return ShowStatusChange(source);
           yield return ShowStatusChange(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {   
        if(state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        //Se um pokemon estiver sob status como burn ou poison, retira vida ao fim de seu turno.
       sourceUnit.Pokemon.OnAfterTurn();
       yield return ShowStatusChange(sourceUnit.Pokemon);
       yield return sourceUnit.Hud.UpdateHP();
       if(sourceUnit.Pokemon.HP <= 0)
       {
          yield return HandlePokemonFainted(sourceUnit);
          yield return new WaitUntil(() => state == BattleState.RunningTurn);
       }
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if(move.Base.AlwaysHits)
            return true;
        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f , 5f / 3f, 2f , 7f / 3f, 8f / 3f, 3f };

        if(accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else    
            moveAccuracy /= boostValues[-accuracy];

        if(evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else    
            moveAccuracy *= boostValues[-evasion];

        
        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChange(Pokemon pokemon)
    {
        while (pokemon.StatusChange.Count > 0)
        {
            var message = pokemon.StatusChange.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name} Fainted");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if(!faintedUnit.IsPlayerUnit)
        {
            //ganha XP
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = (isTrainerBattle)? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();
            //Checagem de level

            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} growed to level {playerUnit.Pokemon.Level}");
           
                //aprendendo novas técnicas
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrLevel();
                if(newMove != null)
                {
                    if(playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        //pergunta se quer esquecer uma técnica caso já possua as 4
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {PokemonBase.MaxNumOfMoves} moves");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                    }
                }
            
                yield return playerUnit.Hud.SetExpSmooth(true);
            }


            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        //Se o pokemon do player for derrotado
        if(faintedUnit.IsPlayerUnit)
        {
            //escolhe o proximo pokemon
            var nextPokemon = playerParty.GetHealthyPokemon();
            if(nextPokemon != null)
                OpenPartyScreen();
            //se não tiver mais pokemons, acaba a batalha
            else
                BattleOver(false);
        }
        else
        {
            //Se for batalha contra pokemon selvagem e o pokemon selvagem for derrotado
            if(!isTrainerBattle) {
                //acaba a batalha
                BattleOver(true);
            }
            else
            {//caso contrário, caso o treinador adversário ainda tiver pokemons
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if(nextPokemon != null)
                    //envia o próximo
                   StartCoroutine(AboutToUse(nextPokemon));
                else
                    //Acaba a batalha
                    BattleOver(true);
            }
            
        }
    }

   IEnumerator ShowDamageDetails(DamageDetails damageDetails)   {
       if(damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if(damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective");
        else if(damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective");
   }

   public void HandleUpdate()
   {
       if(state == BattleState.ActionSelection)
       {
           HandleActionSelection();
       }
       else if (state == BattleState.MoveSelection)
       {
           HandleMoveSelection();
       }
       else if (state == BattleState.PartyScreen)
       {
           HandlePartySelection();
       }
       else if (state == BattleState.AboutToUse)
       {
           HandleAboutToUse();
       }
       else if (state == BattleState.MoveToForget)
       {
           Action<int> onMoveSelected = (moveIndex) =>
           {
               moveSelectionUI.gameObject.SetActive(false);
               if(moveIndex == PokemonBase.MaxNumOfMoves)
               {
                   //Não aprende a nova técnica
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} did not learn {moveToLearn.Name}"));
               }
               else
               {
                   //esquece a técnica escolhida para aprender outra
                   var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                   StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));
                   playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
               }

               moveToLearn = null;
               state = BattleState.RunningTurn;
           };
           moveSelectionUI.HandleMoveSelection(onMoveSelected);
       }
       
   }

   void HandleActionSelection()
   {
       if(Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if(Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if(Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

       dialogBox.UpdateActionSeletion(currentAction);

       if(Input.GetKeyDown(KeyCode.Z))
       {
           if(currentAction == 0)
           {
                MoveSelection();
           }
           else if(currentAction == 1)
           {
               //Bag
               StartCoroutine(RunTurns(BattleAction.UseItem));
           }
           else if(currentAction == 2)
           {
               //Pokemon
               OpenPartyScreen();
           }
           else if(currentAction == 3)
           {
               //Run
               StartCoroutine(RunTurns(BattleAction.Run));
           }
       }
   }
   void HandleMoveSelection() {
        if(Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove;
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove;
        else if(Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if(Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);
        
            dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

            if(Input.GetKeyDown(KeyCode.Z))
            {
                var move = playerUnit.Pokemon.Moves[currentMove];
                if(move.PP == 0) return;

                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(RunTurns(BattleAction.Move));
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                ActionSelection();
            }
        }

        void HandlePartySelection()
        {
            Action onSelected = () =>
            {
                var selectedMember = partyScreen.selectedMember;
            if(selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted pokemon");
                return;
            }
            if(selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't switch with the same pokemon");
                return;
            }
            partyScreen.gameObject.SetActive(false);

            if(partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else{
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
            };
            Action onBack = () =>
            {
                if(playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a pokemon to continue");
                return;
            }
            
            partyScreen.gameObject.SetActive(false);

            if(partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
                ActionSelection();

            partyScreen.CalledFrom = null;
            };

            partyScreen.HandleUpdate(onSelected, onBack);
    }   //pergunta se o player quer trocar o pokemon após derrotar um pokemon do treinador adversário
        void HandleAboutToUse()
        {   //escolhe sim ou não
            if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
                aboutToUseChoice = !aboutToUseChoice;

                dialogBox.UpdateCoiceBox(aboutToUseChoice);
                if(Input.GetKeyDown(KeyCode.Z))
                {
                    dialogBox.EnableChoiceBox(false);
                    if(aboutToUseChoice == true)
                    {
                        //player escolhe outro pokemon
                        OpenPartyScreen();
                    }
                    else
                    {
                        //Treinador envia outro pokemon
                       StartCoroutine(SendNextTrainerPokemon());
                    }
                }
                else if( Input.GetKeyDown(KeyCode.X)) {
                    dialogBox.EnableChoiceBox(false);
                    StartCoroutine(SendNextTrainerPokemon());
                }
        }

        IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse=false)
        {
            if(playerUnit.Pokemon.HP > 0)
            {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
            }
            playerUnit.Setup(newPokemon);
            dialogBox.SetMoveNames(newPokemon.Moves);
            yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

            if(isTrainerAboutToUse)
                StartCoroutine(SendNextTrainerPokemon());
            else
                state = BattleState.RunningTurn;
        }

        IEnumerator SendNextTrainerPokemon()
        {
            state = BattleState.Busy;

            var nextPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(nextPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextPokemon.Base.Name}!");

            state = BattleState.RunningTurn;
        }
        
        IEnumerator ThrowPokeball()
        {
            state = BattleState.Busy;

            if(isTrainerBattle)
            {
                yield return dialogBox.TypeDialog($"You can't steal the trainers pokemon!");
                state = BattleState.RunningTurn;
                yield break;
            }

            yield return dialogBox.TypeDialog($"{player.Name} used POKEBALL!");

            var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
            var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

            //Animações da pokeball
            yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
            yield return enemyUnit.PlayCaptureAnimation();
            yield return pokeball.transform.DOLocalMoveY(enemyUnit.transform.position.y - 1.8f, 0.5f).WaitForCompletion();
        
            int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon);

            for(int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
            {
                yield return new WaitForSeconds(0.5f);
               yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
            }

            if(shakeCount == 4)
            {
                //Pega o pokemon
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} was caught");
                yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

                playerParty.AddPokemon(enemyUnit.Pokemon);
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} has been added to your party");
                
                Destroy(pokeball, 1f);
                BattleOver(true);
            }
            else
            {
                //pokemon quebra a pokeball
                yield return new WaitForSeconds(1f);
                pokeball.DOFade(0, 0.2f);
                yield return enemyUnit.PlayBreakOutAnimation();

                if(shakeCount < 2)
                    yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} broke free");
                else
                    yield return dialogBox.TypeDialog($"Almost caught it");

                    Destroy(pokeball, 1f);
                    state = BattleState.RunningTurn;
            }
        }
    int TryToCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp); 
        
        if(a >= 255)
            return 4;
        
        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(167711680 / a));

        int shakeCount = 0;
        while(shakeCount < 4)
        {
            if(UnityEngine.Random.Range(0, 65535) >= b)
                break;
                    
            shakeCount++;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;
        if(isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }
        
        ++escapeAttemps;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if(enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Run away safely");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttemps;
            f = f % 256;

            if(UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Run away safely");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape");
                state = BattleState.RunningTurn;
            }
        }
    }
}