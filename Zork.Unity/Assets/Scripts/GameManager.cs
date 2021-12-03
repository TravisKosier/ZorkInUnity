using Newtonsoft.Json;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using Zork.Common;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI CurrentLocationText;
    [SerializeField]
    private TextMeshProUGUI MovesText;
    [SerializeField]
    private TextMeshProUGUI ScoreText;
    [SerializeField]
    private UnityInputService InputService;
    [SerializeField]
    private UnityOutputService OutputService;
    

    // Start is called before the first frame update
    void Start()
    {
        TextAsset gameTextAsset = Resources.Load<TextAsset>("Zork");
        _game = JsonConvert.DeserializeObject<Game>(gameTextAsset.text);
        _game.Player.LocationChanged += PlayerLocationChanged;
        _game.Player.ScoreChanged += ScoreChanged;
        _game.Player.MovesChanged += MovesChanged;
        _game.Player.HasQuitChanged += HasQuitChanged;

        _game.Start(InputService, OutputService);
        CurrentLocationText.text = _game.Player.Location.ToString();
        _game.Commands["LOOK"].Action(new CommandContext(_game,""));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void MovesChanged(object sender, int newMoves)
    {
        MovesText.text = $"Moves: {newMoves.ToString()}";
        
    }

    private void ScoreChanged(object sender, int newScore)
    {
        ScoreText.text = $"Score: {newScore.ToString()}";
        OutputService.WriteLine($"Your score increased to {newScore}!");
    }


    private void PlayerLocationChanged(object sender, Room newLocation)
    {
        CurrentLocationText.text = newLocation.ToString();
        OutputService.WriteLine($"You moved to {newLocation.ToString()}.");
        _game.Commands["LOOK"].Action(new CommandContext(_game, ""));
    }

    private void HasQuitChanged(object sender, bool newHasQuit)
    {
        if (newHasQuit)
        {
            OutputService.WriteLine("Thank you for playing!");
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
    }

    private Game _game;
    private Room _previousLocation;
}
