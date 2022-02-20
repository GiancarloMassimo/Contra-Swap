using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

/* 
 * Hi Alejandro,
 * 
 * If you're reading this, I sincerely apologize, but there is not
 * a chance in hell that I'm going to write clean code for a 38
 * hour project.
 * 
 * Thank you for understanding,
 * Giancarlo 
 */

public class GameBoardController : MonoBehaviour
{
    [SerializeField]
    int level;

    [SerializeField]
    int swapsLeft;

    [SerializeField]
    bool canSwapRules;

    [SerializeField]
    GameObject tile;

    [SerializeField]
    Vector2 startingPoint;

    [SerializeField]
    float tileSpacing;

    [SerializeField]
    GameObject rowCondition = null, colCondition = null, boxCondition = null;

    [SerializeField]
    TMP_Text swapsLeftText;

    [SerializeField]
    Animator loseAnim;

    bool levelOver = false;

    readonly int[,] Level1 = new int[,] {
        {1, 2, 7, 5},
        {2, 1, 3, 4},
    };

    readonly int[,] Level2 = new int[,] {
        {2, 4, 1, 4},
        {1, 3, 4, 2},
        {3, 2, 4, 1},
        {3, 1, 3, 2}
    };

    readonly int[,] Level3 = new int[,] {
        {1, 1, 0, 1},
        {1, 0, 0, 1},
        {0, 0, 0, 1},
        {0, 1, 0, 1}
    };

    readonly int[,] Level4 = new int[,] {
        {4, 3, 2, 1},
        {8, 7, 6, 5}
    };

    readonly int[,] Level5 = new int[,] {
        {-1, 3, -1, -1},
        {2, 5, 1, -1},
        {2, 1, 2, 3},
        {5, -1, 8, -1}
    };

    readonly int[,] Level6 = new int[,] {
        {0, 1, 6, 1},
        {3, 4, 0, 0},
        {9, 1, 1, 3},
        {4, 2, 2, 7}
    };


    GameObject[,] levelMatrix;

    GameObject objectSelected;

    [SerializeField]
    PostProcessVolume volume;
    ColorGrading colorGrading;

    AudioController sounds;

    void Start()
    {
        volume.profile.TryGetSettings(out colorGrading);
        colorGrading.enabled.value = false;

        sounds = GameObject.FindGameObjectWithTag("AudioPlayer").GetComponent<AudioController>();

        int[,] startingNums = null;
        switch (level)
        {
            case 1:
                startingNums = Level1;
                break;
            case 2:
                startingNums = Level2;
                break;
            case 3:
                startingNums = Level3;
                break;
            case 4:
                startingNums = Level4;
                break;
            case 5:
                startingNums = Level5;
                break;
            case 6:
                startingNums = Level6;
                break;
            default:
                Debug.LogError("Level does not exist");
                break;
        }

        levelMatrix = new GameObject[startingNums.GetLength(0), startingNums.GetLength(1)];

        for (int r = 0; r < levelMatrix.GetLength(0); r++)
        {
            for (int c = 0; c < levelMatrix.GetLength(1); c++)
            {
                if (startingNums[r, c] == -1)
                {
                    continue;
                }

                GameObject newTile = Instantiate(tile,
                    new Vector2(startingPoint.x + c * tileSpacing, startingPoint.y - r * tileSpacing),
                    Quaternion.identity);

                TMP_Text tileText = newTile.transform.GetChild(0).GetComponent<TMP_Text>();
                tileText.text = startingNums[r, c] + "";

                levelMatrix[r, c] = newTile;
            }
        }
        swapsLeftText.text = "Swaps Left: " + swapsLeft;
        CheckConditions();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetMouseButtonDown(0) && !levelOver)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            GameObject objectClicked = !hit ? null : hit.transform.gameObject;

            if (objectClicked == null)
            {
                if (objectSelected != null)
                {
                    if (objectSelected.CompareTag("Tile"))
                    {
                        objectSelected.GetComponent<TileController>().OnDeselect();
                    }
                    else
                    {
                        objectSelected.GetComponent<Condition>().OnDeselect();
                    }
                    objectSelected = null;
                }
            }
            else
            {
                if (objectClicked.CompareTag("Tile"))
                {
                    if (objectSelected == null || !objectSelected.CompareTag("Tile"))
                    {
                        if (objectSelected != null)
                        {
                            objectSelected.GetComponent<Condition>().OnDeselect();
                        }
                        objectSelected = objectClicked;
                        sounds.Select();
                        objectClicked.GetComponent<TileController>().OnSelect();
                    }
                    else if (objectSelected != null && objectSelected.CompareTag("Tile"))
                    {
                        sounds.Swap();
                        SwapTiles(objectClicked);
                    }
                } 
                else if (canSwapRules && objectClicked.CompareTag("Rule"))
                {
                    if (objectSelected == null || !objectSelected.CompareTag("Rule"))
                    {
                        if (objectSelected != null)
                        {
                            objectSelected.GetComponent<TileController>().OnDeselect();
                        }

                        objectSelected = objectClicked;
                        sounds.Select();
                        objectClicked.GetComponent<Condition>().OnSelect();
                    }
                    else if (objectSelected != null && objectSelected.CompareTag("Rule"))
                    {
                        sounds.Swap();
                        SwapRules(objectClicked);
                    }
                }
            }
        }
    }

    void SwapTiles(GameObject clicked)
    {
        TileController a = objectSelected.GetComponent<TileController>();
        a.OnDeselect();
        TileController b = clicked.GetComponent<TileController>();
        Vector2 temp = a.DesiredPosition;
        a.DesiredPosition = b.DesiredPosition;
        b.DesiredPosition = temp;

        int[] indexA = IndexOfTile(a.gameObject);
        int[] indexB = IndexOfTile(b.gameObject);

        levelMatrix[indexA[0], indexA[1]] = b.gameObject;
        levelMatrix[indexB[0], indexB[1]] = a.gameObject;

        objectSelected = null;
        swapsLeft--;
        swapsLeftText.text = "Swaps Left: " + swapsLeft;
        if (CheckConditions())
        {
            NextLevel();
        }
        else if (swapsLeft <= 0)
        {
            LoseLevel();
        }
    }

    void SwapRules(GameObject clicked)
    {
        Condition a1 = objectSelected.GetComponent<Condition>();
        a1.OnDeselect();
        Condition b1 = clicked.GetComponent<Condition>();
        Vector2 temp = a1.DesiredPosition;
        a1.DesiredPosition = b1.DesiredPosition;
        b1.DesiredPosition = temp;

        GameObject a = objectSelected, b = clicked;

        if (a == rowCondition)
        {
            if (b == colCondition)
            {
                colCondition = a;
                rowCondition = b;
            }
            else
            {
                boxCondition = a;
                rowCondition = b;
            }
        }
        else if (a == colCondition)
        {
            if (b == rowCondition)
            {
                rowCondition = a;
                colCondition = b;
            }
            else
            {
                boxCondition = a;
                colCondition = b;
            }
        }
        else
        {
            if (b == rowCondition)
            {
                rowCondition = a;
                boxCondition = b;
            }
            else
            {
                colCondition = a;
                boxCondition = b;
            }
        }

        objectSelected = null;

        swapsLeft--;
        swapsLeftText.text = "Swaps Left: " + swapsLeft;
        if (CheckConditions())
        {
            NextLevel();
        } 
        else if (swapsLeft <= 0)
        {
            LoseLevel();
        }
    }

    int[] IndexOfTile(GameObject tile)
    {
        for (int r = 0; r < levelMatrix.GetLength(0); r++)
        {
            for (int c = 0; c < levelMatrix.GetLength(1); c++)
            {
                if (levelMatrix[r, c] == tile)
                {
                    return new int[] { r, c };
                }
            }
        }

        return null;
    }

    void NextLevel()
    {
        levelOver = true;
        colorGrading.enabled.value = true;
        sounds.Win();
        StartCoroutine(DelaySceneLoad());
    }

    void LoseLevel()
    {
        levelOver = true;
        sounds.Lose();
        loseAnim.SetTrigger("Lose");
    }

    IEnumerator DelaySceneLoad()
    {
        yield return new WaitForSeconds(1.5f);
        LoadScene(level == 6 ? "Menu" : "Level" + (level + 1));
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    bool CheckConditions()
    {
        bool passesAll = true;

        if (rowCondition != null)
        {
            Condition condition = rowCondition.GetComponent<Condition>();
            bool passes = true;
            if (condition is SameSumCondition)
            {
                int[,] check = new int[levelMatrix.GetLength(0), levelMatrix.GetLength(1)];
                for (int r = 0; r < levelMatrix.GetLength(0); r++)
                {
                    for (int c = 0; c < levelMatrix.GetLength(1); c++)
                    {
                        if (levelMatrix[r, c] == null)
                        {
                            check[r, c] = -1;
                            continue;
                        }
                        check[r, c] = GetNumberFromTile(levelMatrix[r, c]);
                    }
                }

                passes = AddToSameNumber(check);
            } 
            else
            {
                for (int r = 0; r < levelMatrix.GetLength(0); r++)
                {
                    int[] check = new int[levelMatrix.GetLength(1)];
                    for (int c = 0; c < levelMatrix.GetLength(1); c++)
                    {
                        if (levelMatrix[r, c] == null)
                        {
                            check[c] = -1;
                            continue;
                        }
                        check[c] = GetNumberFromTile(levelMatrix[r, c]);
                    }
                    if (!condition.MeetsCondition(check))
                    {
                        passes = false;
                        break;
                    }
                }
            }

            if (passes)
            {
                condition.AdjustColorIndicator(true);
            }
            else
            {
                passesAll = false;
                condition.AdjustColorIndicator(false);
            }
        }

        if (colCondition != null)
        {
            Condition condition = colCondition.GetComponent<Condition>();
            bool passes = true;
            if (condition is SameSumCondition)
            {
                int[,] check = new int[levelMatrix.GetLength(1), levelMatrix.GetLength(0)];
                for (int c = 0; c < levelMatrix.GetLength(1); c++)
                {
                    for (int r = 0; r < levelMatrix.GetLength(0); r++)
                    {
                        if (levelMatrix[r, c] == null)
                        {
                            check[c, r] = -1;
                            continue;
                        }
                        check[c, r] = GetNumberFromTile(levelMatrix[r, c]);
                    }
                }

                passes = AddToSameNumber(check);
            }
            else
            {
                for (int c = 0; c < levelMatrix.GetLength(1); c++)
                {
                    int[] check = new int[levelMatrix.GetLength(0)];
                    for (int r = 0; r < levelMatrix.GetLength(0); r++)
                    {
                        if (levelMatrix[r, c] == null)
                        {
                            check[r] = -1;
                            continue;
                        }
                        check[r] = GetNumberFromTile(levelMatrix[r, c]);
                    }
                    if (!condition.MeetsCondition(check))
                    {
                        passes = false;
                        break;
                    }
                }
            }

            if (passes)
            {
                condition.AdjustColorIndicator(true);
            }
            else
            {
                passesAll = false;
                condition.AdjustColorIndicator(false);
            }

        }

        if (boxCondition != null)
        {
            Condition condition = boxCondition.GetComponent<Condition>();
            bool passes = true;

            int[] box1 = { GetNumberFromTile(levelMatrix[0, 0]), GetNumberFromTile(levelMatrix[0, 1]),
                           GetNumberFromTile(levelMatrix[1, 0]), GetNumberFromTile(levelMatrix[1, 1])};

            int[] box2 = { GetNumberFromTile(levelMatrix[0, 2]), GetNumberFromTile(levelMatrix[0, 3]),
                           GetNumberFromTile(levelMatrix[1, 2]), GetNumberFromTile(levelMatrix[1, 3])};

            int[] box3 = { GetNumberFromTile(levelMatrix[2, 0]), GetNumberFromTile(levelMatrix[2, 1]),
                           GetNumberFromTile(levelMatrix[3, 0]), GetNumberFromTile(levelMatrix[3, 1])};

            int[] box4 = { GetNumberFromTile(levelMatrix[2, 2]), GetNumberFromTile(levelMatrix[2, 3]),
                           GetNumberFromTile(levelMatrix[3, 2]), GetNumberFromTile(levelMatrix[3, 3])};


            if (condition is SameSumCondition)
            {
                int[,] check = new int[4, 4];

                for (int i = 0; i < 4; i++)
                {
                    check[0, i] = box1[i];
                }
                for (int i = 0; i < 4; i++)
                {
                    check[1, i] = box2[i];
                }
                for (int i = 0; i < 4; i++)
                {
                    check[2, i] = box3[i];
                }
                for (int i = 0; i < 4; i++)
                {
                    check[3, i] = box4[i];
                }

                passes = AddToSameNumber(check);
            }
            else
            {
                passes = condition.MeetsCondition(box1) && condition.MeetsCondition(box2) && condition.MeetsCondition(box3)
                    && condition.MeetsCondition(box4);
            }

            if (passes)
            {
                condition.AdjustColorIndicator(true);
            }
            else
            {
                passesAll = false;
                condition.AdjustColorIndicator(false);
            }
        }

        return passesAll;
    }

    int GetNumberFromTile(GameObject tile)
    {
        return int.Parse(tile.transform.GetChild(0).GetComponent<TMP_Text>().text);
    }

    bool AddToSameNumber(int[,] mat)
    {
        bool passes = true;

        int sameSum = -1;

        for (int r = 0; r < mat.GetLength(0); r++)
        {
            int sum = 0;
            for (int c = 0; c < mat.GetLength(1); c++) {
                if (mat[r, c] == -1)
                {
                    continue;
                }
                sum += mat[r, c];
            }

            //Debug.Log(sum);

            if (r == 0)
            {
                sameSum = sum;
            }
            else if (sum != sameSum) 
            {
                passes = false;
                break;
            }
        }
        //Debug.Log("end check");

        return passes;
    }
}
