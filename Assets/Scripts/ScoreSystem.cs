using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSystem : MonoBehaviour
{
    public GameObject trickScoreParent;
    public GameObject trickScorePrefab;
    public GameObject trickScoreInstantPrefab;

    public GameObject scoreIncreaseParent;
    public GameObject scoreIncreasePrefab;

    public GameObject scoreAttackIncreaseParent;
    public GameObject scoreAttackIncreasePrefab;

    public Animator scoreAnimator;
    public Animator currentTrickAnimator;
    public Animator rankingIconAnimator;

    public Text currentTrickScoreText;
    public Text currentScoreText;
    public Text currentScoreAttackText;
    public Text currentHighScoreAttackText;

    public AudioSource counterAudioSource;
    public AudioSource trickCompleteAudioSource;

    public AnimationCurve counterPitchCurve;

    private float minimumTrickScore = 10f;
    private float tickMultiplier = 3f;
    
    private float _currentUpdateTrickScore;
    private float _currentScore;
    private bool _isScoring = false;
    private float _currentTrickTime;
    private string _currentTrickDescription;
    private bool _muteScores = false;
    private bool _isScoreAttack = false;
    //private List<TrialScores> _currentTrialScores;

    private Queue<Trick> trickQueue;
    private float _carryOverScore;

    private void Start()
    {
        minimumTrickScore *= tickMultiplier;

        trickQueue = new Queue<Trick>();
        StartCoroutine(C_UpdateTrickQueue());

        if (_carryOverScore > 0f)
        {
            _currentScore = _carryOverScore;
            SetCurrentScore();
            _carryOverScore = 0f;
        }
    }

    public void StartTrick (string description = "")
    {
        if (_muteScores) return;
        if (_isScoring) EndTrick();
        _currentTrickDescription = description;
        _currentUpdateTrickScore = 0f;
        _currentTrickTime = 0f;
        _isScoring = true;
        currentTrickAnimator.SetBool ("Scoring", true);
        if (!counterAudioSource.isPlaying)
            counterAudioSource.Play();
    }

    public void EndTrick (bool cancelTrick = false)
    {
        if (_muteScores) return;
        _isScoring = false;
        currentTrickAnimator.SetBool ("Scoring", false);
        counterAudioSource.Stop();

        float score = _currentUpdateTrickScore;
        AddTrickToQueue(_currentTrickDescription, score, TrickType.Normal);

        Mission currentMission = GameManager.Instance.missionManagerHUD.GetCurrentMission();

        if (currentMission != null)
        {
            if (currentMission.type == MissionType.SingleDrift || currentMission.type == MissionType.DriftTimeCount)
            {
                if (score >= currentMission.targetValue && currentMission.type == MissionType.SingleDrift)
                {
                    GameManager.Instance.missionManagerHUD.UpdateMissionProgress(MissionType.SingleDrift, currentMission.targetValue);
                    return;
                }
                GameManager.Instance.missionManagerHUD.UpdateMissionProgress(MissionType.DriftTimeCount, (int)score);
                return;
            }
            else if (currentMission.type == MissionType.AirTimeCount)
            {
                GameManager.Instance.missionManagerHUD.UpdateMissionProgress(MissionType.AirTimeCount, (int)score);
                return;
            }
        }
    }

    public void InstantTrick(float points, string description)
    {
        AddTrickToQueue(description, points, TrickType.Instant);
    }

    public void BumpScore(float points)
    {
        scoreAnimator.SetTrigger("Bump");
        _currentScore += points;
        SetCurrentScore();
    }

    public float GetScore()
    {
        return _currentScore;
    }

    public void SetScore(float newScore)
    {
        _currentScore = newScore;
        SetCurrentScore();
    }

    public void ResetCurrentScore()
    {
        _currentScore = 0f;
        SetCurrentScore();
    }
    
    void CreateScoreIncrease (float points)
    {
        if (_muteScores) return;
        if (points < minimumTrickScore) return;

        GameObject scoreIncreaseObject = Instantiate (scoreIncreasePrefab, Vector3.zero, Quaternion.identity, scoreIncreaseParent.transform);
        scoreIncreaseObject.transform.localPosition = Vector3.zero;
        ScoreIncrease scoreIncrease = scoreIncreaseObject.GetComponent<ScoreIncrease> ();
        scoreIncrease.text.text = string.Format ("{0:n0}", points);
        scoreIncrease.points = points;

        if (!_isScoreAttack) return;

        GameObject scoreAttackIncreaseObject = Instantiate (scoreAttackIncreasePrefab, Vector3.zero, Quaternion.identity, scoreAttackIncreaseParent.transform);
        scoreAttackIncreaseObject.transform.localPosition = Vector3.zero;
        ScoreIncrease scoreAttackIncrease = scoreAttackIncreaseObject.GetComponent<ScoreIncrease> ();
        scoreAttackIncrease.text.text = string.Format ("{0:n0}", points);
        scoreAttackIncrease.points = points;
    }

    void Update ()
    {
        if (_muteScores) return;

        if (_isScoring)
        {
            _currentUpdateTrickScore += 100f * Time.deltaTime * tickMultiplier;
            _currentTrickTime += Time.deltaTime;
            counterAudioSource.pitch = counterPitchCurve.Evaluate(_currentTrickTime);
            currentTrickScoreText.text = string.Format("{0:n0}", _currentUpdateTrickScore);
        }
    }

    void AddTrickToQueue(string description, float score, TrickType type)
    {
        Trick newTrick = new Trick(description, score, type);
        trickQueue.Enqueue(newTrick);
    }

    IEnumerator C_UpdateTrickQueue()
    {
        while (true)
        {
            if (trickQueue.Count <= 0)
            {
                yield return null;
                continue;
            }

            Trick nextTrick = trickQueue.Dequeue();
            CreateTrickScoreFromQueue(nextTrick);
            CreateScoreIncrease(nextTrick.score);
            yield return new WaitForSeconds(1f);
        }
    }

    void CreateTrickScoreFromQueue (Trick trick)
    {
        if (_muteScores) return;
        if (trick.score < minimumTrickScore) return;

        GameObject prefab = (trick.type == TrickType.Normal) ? trickScorePrefab : trickScoreInstantPrefab;

        GameObject trickScoreObject = Instantiate (prefab, Vector3.zero, Quaternion.identity, trickScoreParent.transform);
        trickScoreObject.transform.localPosition = Vector3.zero;
        TrickScore trickScore = trickScoreObject.GetComponent<TrickScore> ();
        trickScore.text.text = string.Format ("{0:n0}", trick.score);
        trickScore.description.text = trick.description.ToUpper();
        trickScore.animator.SetTrigger ("Normal");
        trickCompleteAudioSource.Play();
    }

    public void MuteScores(bool value)
    {
        if (_isScoring)
        {
            _isScoring = false;
            currentTrickAnimator.SetBool ("Scoring", false);
            counterAudioSource.Stop();
        }
        _muteScores = value;
    }

    public void SetCarryOverScore()
    {
        _carryOverScore = _currentScore;
    }

    private void SetCurrentScore()
    {
        string newString = string.Format ("{0:n0}", _currentScore);

        currentScoreText.text = newString;
        currentScoreAttackText.text = newString;

        if (!_isScoreAttack) return;

        //int scoresBeaten = _currentTrialScores.Where(x => x.score < _currentScore).Count();
        //rankingIconAnimator.SetInteger("RankingProgression", scoresBeaten);
    }
}

public class Trick
{
    public string description;
    public float score;
    public TrickType type;

    public Trick(string _description, float _score, TrickType _type)
    {
        description = _description;
        score = _score;
        type = _type;
    }
}

public enum TrickType
{
    Normal,
    Instant,
}