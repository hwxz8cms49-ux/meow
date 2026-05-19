using NUnit.Framework;
using System.Collections;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class _Board : MonoBehaviour
{
    public int width = 9;
    public int height = 14;
    public GameObject[] Prefabs;
    private GameObject[,] AllAnimals;
    private GameObject CurrentAnimal;
    float spacingX = 100f;
    float spacingY = 100f;
    float offsetX;
    float offsetY;
    public Text Score_;
    public TextMeshProUGUI Timer_;
    private int score = 0;
    private float timer = 60f;
    private bool IsGameOver = false;
    public GameObject GameOverPan;
    public TextMeshProUGUI FinalScore;
    public TextMeshProUGUI BestScore;
    void Start()
    {
        AllAnimals = new GameObject[width, height];
        CreateBoard();
    }
    void CreateBoard()
    {
        offsetX = (width - 1) * spacingX / 2f;
        offsetY = (height - 1) * spacingY / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int RandomIndex = Random.Range(0, Prefabs.Length);
                while (CoincidencesNearby(x, y, Prefabs[RandomIndex]))
                {
                    RandomIndex = Random.Range(0, Prefabs.Length);
                }

                Vector2 pos = new Vector2(x * spacingX - offsetX, y * spacingY - offsetY);

                GameObject Tile = Instantiate(Prefabs[RandomIndex], transform);
                Tile.GetComponent<RectTransform>().anchoredPosition = pos; 

                AllAnimals[x, y] = Tile;

                if (Tile.GetComponent<_InteractionWithAnimals>() != null)
                {
                    var script = Tile.GetComponent<_InteractionWithAnimals>();
                    script.x = x;
                    script.y = y;
                }
            }
        }
    }

    bool CoincidencesNearby(int x, int y, GameObject Dot)
    {
        if (x > 1)
        {
            if (AllAnimals[x - 1, y] != null && AllAnimals[x - 2, y] != null)
            {
                if (AllAnimals[x - 1, y].tag == Dot.tag && AllAnimals[x - 2, y].tag == Dot.tag)
                {
                    return true;
                }
            }
        }
        if (y > 1)
        {
            if (AllAnimals[x, y - 1] != null && AllAnimals[x, y - 2] != null)
            {
                if (AllAnimals[x, y - 1].tag == Dot.tag && AllAnimals[x, y - 2].tag == Dot.tag)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void SelectAnimal(GameObject pet)
    {
        if (IsGameOver == true)
        {
            return;
        }
        if (CurrentAnimal == null)
        {
            CurrentAnimal = pet;
        }
        else
        {
            int x1 = CurrentAnimal.GetComponent<_InteractionWithAnimals>().x;
            int x2 = pet.GetComponent<_InteractionWithAnimals>().x;
            int y1 = CurrentAnimal.GetComponent<_InteractionWithAnimals>().y;
            int y2 = pet.GetComponent<_InteractionWithAnimals>().y;
            int calcX = Mathf.Abs(x1 - x2);
            int calcY = Mathf.Abs(y1 - y2);
            if ((calcX == 1 && calcY == 0) || (calcX == 0 && calcY == 1))
            {
                StartCoroutine(SwapAnimals(CurrentAnimal, pet));
            }
            CurrentAnimal = null;
        }
    }
    IEnumerator SwapAnimals(GameObject pet1, GameObject pet2)
    {
        int CurrX1 = pet1.GetComponent<_InteractionWithAnimals>().x;
        int CurrX2 = pet2.GetComponent<_InteractionWithAnimals>().x;
        int CurrY2 = pet2.GetComponent<_InteractionWithAnimals>().y;
        int CurrY1 = pet1.GetComponent<_InteractionWithAnimals>().y;

        GameObject box = AllAnimals[CurrX1, CurrY1];
        AllAnimals[CurrX1, CurrY1] = AllAnimals[CurrX2, CurrY2];
        AllAnimals[CurrX2, CurrY2] = box;

        pet1.GetComponent<_InteractionWithAnimals>().x = CurrX2;
        pet2.GetComponent<_InteractionWithAnimals>().x = CurrX1;
        pet1.GetComponent<_InteractionWithAnimals>().y = CurrY2;
        pet2.GetComponent<_InteractionWithAnimals>().y = CurrY1;

        pet1.GetComponent<RectTransform>().anchoredPosition = new Vector2(CurrX2 * spacingX - offsetX, CurrY2 * spacingY - offsetY);
        pet2.GetComponent<RectTransform>().anchoredPosition = new Vector2(CurrX1 * spacingX - offsetX, CurrY1 * spacingY - offsetY);

        List<GameObject> MatchPet1 = CheckMatch(pet1);
        List<GameObject> MatchPet2 = CheckMatch(pet2);
        if (MatchPet1 == null && MatchPet2 == null)
        {
            yield return new WaitForSeconds(0.5f);
            AllAnimals[CurrX1, CurrY1] = pet1;
            AllAnimals[CurrX2, CurrY2] = pet2;
            pet1.GetComponent<_InteractionWithAnimals>().x = CurrX1;
            pet2.GetComponent<_InteractionWithAnimals>().x = CurrX2;
            pet1.GetComponent<_InteractionWithAnimals>().y = CurrY1;
            pet2.GetComponent<_InteractionWithAnimals>().y = CurrY2;

            pet1.GetComponent<RectTransform>().anchoredPosition = new Vector2(CurrX1 * spacingX - offsetX, CurrY1 * spacingY - offsetY);
            pet2.GetComponent<RectTransform>().anchoredPosition = new Vector2(CurrX2 * spacingX - offsetX, CurrY2 * spacingY - offsetY);
        }
        else if ( MatchPet1 != null )
        {
            foreach (GameObject MatchPet in MatchPet1)
            {
                int x1 = MatchPet.GetComponent<_InteractionWithAnimals>().x;
                int y1 = MatchPet.GetComponent<_InteractionWithAnimals>().y;
                AllAnimals[x1, y1] = null;
                Destroy(MatchPet);
                score += 10;
                Score_.text = score.ToString();
            }
            StartCoroutine(DropAnimals());
        }

        else if (MatchPet2 != null) 
        {
            foreach (GameObject MatchPet in MatchPet2)
            {
                int x2 = MatchPet.GetComponent<_InteractionWithAnimals>().x;
                int y2 = MatchPet.GetComponent<_InteractionWithAnimals>().y;
                AllAnimals[x2, y2] = null;
                Destroy(MatchPet);
                score += 10;
                Score_.text = score.ToString();
            }
            StartCoroutine(DropAnimals());
        }

        yield return null;
    }

    List<GameObject> CheckMatch(GameObject pet)
    {
        List<GameObject> MatchList = new List<GameObject>();
        MatchList.Add(pet);
        string CurrTag = pet.tag;
        int x = pet.GetComponent<_InteractionWithAnimals>().x;
        int y = pet.GetComponent<_InteractionWithAnimals>().y;
        for (int i = x - 1; i >= 0; i--)
        {
            if (AllAnimals[i, y].tag == CurrTag)
            {
                MatchList.Add(AllAnimals[i, y]);
            }
            else
            {
                break;
            }
        }
        for (int i = x + 1; i < width; i++)
        {
            if (AllAnimals[i, y].tag == CurrTag)
            {
                MatchList.Add(AllAnimals[i, y]);
            }
            else
            {
                break;
            }
        }
        if (MatchList.Count >= 3)
        {
            return MatchList;
        }
        else
        {
            MatchList = new List<GameObject>();
            MatchList.Add(pet);
            for (int i = y - 1; i >= 0; i--)
            {
                if (AllAnimals[x, i].tag == CurrTag)
                {
                    MatchList.Add(AllAnimals[x, i]);
                }
                else
                {
                    break;
                }
            }
            for (int i = y + 1; i < height; i++)
            {
                if (AllAnimals[x, i].tag == CurrTag)
                {
                    MatchList.Add(AllAnimals[x, i]);
                }
                else
                {
                    break;
                }
            }
            if (MatchList.Count >= 3)
            {
                return MatchList;
            }
        }

        return null;
    }

    IEnumerator DropAnimals()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (AllAnimals[i, j] == null)
                {
                    for (int h = j + 1; h < height; h++)
                    {
                        if (AllAnimals[i, h] !=  null)
                        {
                            AllAnimals[i, j] = AllAnimals[i, h];
                            AllAnimals[i, h] = null;
                            _InteractionWithAnimals Move = AllAnimals[i, j].GetComponent <_InteractionWithAnimals>();
                            Move.y = j;
                            AllAnimals[i, j].GetComponent<RectTransform>().anchoredPosition = new Vector2(i * spacingX - offsetX, j * spacingY - offsetY);
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
        Refilling();
        yield return null;
    }

    void Refilling()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (AllAnimals[i, j] == null)
                {
                    int RandomIndex = Random.Range(0, Prefabs.Length);
                    while (CoincidencesNearby(i, j, Prefabs[RandomIndex]))
                    {
                        RandomIndex = Random.Range(0, Prefabs.Length);
                    }

                    Vector2 pos = new Vector2(i * spacingX - offsetX, j * spacingY - offsetY);

                    GameObject Tile = Instantiate(Prefabs[RandomIndex], transform);
                    Tile.GetComponent<RectTransform>().anchoredPosition = pos;

                    AllAnimals[i, j] = Tile;

                    if (Tile.GetComponent<_InteractionWithAnimals>() != null)
                    {
                        var script = Tile.GetComponent<_InteractionWithAnimals>();
                        script.x = i;
                        script.y = j;
                    }
                }
            }
        }
        ChekingTheEntieBoard();
    }

    void ChekingTheEntieBoard()
    {
        for (int i =0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (AllAnimals[i, j] != null)
                {
                    List<GameObject> MatchPet = CheckMatch(AllAnimals[i, j]);
                    if (MatchPet != null)
                    {
                        foreach (GameObject MatchPet_ in MatchPet)
                        {
                            int x1 = MatchPet_.GetComponent<_InteractionWithAnimals>().x;
                            int y1 = MatchPet_.GetComponent<_InteractionWithAnimals>().y;
                            AllAnimals[x1, y1] = null;
                            Destroy(MatchPet_);
                        }
                        StartCoroutine(DropAnimals());
                        return;
                    }
                }
            }
        }
    }

    void GameOver()
    {
        IsGameOver = true;
        Timer_.text = "Время истекло.";
        Timer_.gameObject.SetActive(false);
        Score_.gameObject.SetActive(false);
        if (GameOverPan !=  null)
        {
            GameOverPan.SetActive(true);
        }
        if (FinalScore != null)
        {
            FinalScore.text = "Ваш итоговый счет: " + score.ToString();
        }
        int HighestScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > HighestScore)
        {
            HighestScore = score;
            PlayerPrefs.SetInt("HighScore", HighestScore);
            PlayerPrefs.Save();
        }
        if (BestScore  != null)
        {
            BestScore.text = "Лучший результат: " + HighestScore.ToString();
        }
    }
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            Timer_.text = Mathf.RoundToInt(timer).ToString();
        }
        else if (!IsGameOver)
        {
            GameOver();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

