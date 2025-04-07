using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance { get; private set; }
    
    [Header("Dialog References")]
    [SerializeField] private DialogDatabaseSO dialogDatabase;

    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;

    [SerializeField] private Image portraitImage;               // ĳ���� �ʻ�ȭ �̹��� UI ��� �߰�

    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button NextButton;

    [Header("Dialog Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffect = true;

    [Header("Dialog Choices")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private GameObject ChoiceButtonPrefab;

    private bool isTyping = false;
    private Coroutine typingCoroutine;          // �ڷ�ƾ ����

    private DialogSO currentDialog;

    private void Awake()
    {
        if (instance == null)                   // �̱��� ���� ����
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (dialogDatabase != null)
        {
            dialogDatabase.Initailize();        // �ʱ�ȭ
        }
        else
        {
            Debug.LogError("Dialog Database is not assinged to Dialog Manager");
        }
        if(NextButton != null)
        {
            NextButton.onClick.AddListener(NextDialog);       // ��ư ������ ���
        }
        else
        {
            Debug.LogError("Next Button is not assigned!");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        // UI �ʱ�ȭ �� ��ȭ ���� (ID 1)
        CloseDialog();
        StartDialog(1);                 // �ڵ����� ù ��° ��ȭ ����
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ID�� ��ȭ ����
    public void StartDialog(int dialogId)
    {
        DialogSO dialog = dialogDatabase.GetDialogByld(dialogId);
        if(dialog != null)
        {
            StartDialog(dialog);
        }
        else
        {
            Debug.LogError($"Dialog with ID {dialogId} not found!");
        }
    }

    // DialogSO�� ��ȭ ����
    public void StartDialog(DialogSO dialog)
    {
        if (dialog == null) return;

        currentDialog = dialog;
        ShowDialog();
        dialogPanel.SetActive(true);
    }

    public void ShowDialog()
    {
        if (currentDialog == null) return;
        characterNameText.text = currentDialog.characterName;           // ĳ���� �̸� ����

        // ��ȭ �ؽ�Ʈ �κ� ����
        if (useTypewriterEffect)
        {
            StartTypingEffect(currentDialog.text);
        }
        else
        {
            dialogText.text = currentDialog.text;                       // ��ȭ �ؽ�Ʈ ����
        }

        // �ʻ�ȭ ���� (���� �߰��� �κ�)
        if(currentDialog.portrait != null)
        {
            portraitImage.sprite = currentDialog.portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else if (!string.IsNullOrEmpty(currentDialog.portraitPath))
        {
            // Resources �������� �̹��� �ε�
            Sprite portrait = Resources.Load<Sprite>(currentDialog.portraitPath);   // Assets/Resources/Characters/Narrator.png (���� �̹��� ���)
            if(portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Portrait not found at path : {currentDialog.portraitPath}");
                portraitImage.gameObject.SetActive(false);
            }
        }
        else
        {
            portraitImage.gameObject.SetActive(false);          // �ʻ�ȭ�� ������ �̹��� ��Ȱ��ȭ
        }

        // ������ ǥ��
        ClearChoices();
        if (currentDialog.choices != null && currentDialog.choices.Count > 0)
        {
            ShowChoices();
            NextButton.gameObject.SetActive(false);
        }
        else
        {
            NextButton.gameObject.SetActive(true);
        }
    }

    public void NextDialog()            // ���� ��ȭ�� ����
    {
        if(isTyping)        // Ÿ���� ���̸� Ÿ���� �Ϸ� ó��
        {
            StopTypingEffect();
            dialogText.text = currentDialog.text;
            isTyping = false;
            return;
        }

        if(currentDialog != null && currentDialog.nextId > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialogByld(currentDialog.nextId);
            {
                if (nextDialog != null)
                {
                    currentDialog = nextDialog;
                    ShowDialog();
                }
                else
                {
                    CloseDialog();
                }
            }
        }
        else
        {
            CloseDialog();
        }
    }

    // �ؽ�Ʈ Ÿ���� ȿ�� �ڷ�ƾ
    private IEnumerator TypeText(string text)
    {
        dialogText.text = "";
        foreach(char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    // Ÿ���� ȿ�� ����
    private void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    // Ÿ���� ȿ�� ����
    private void StartTypingEffect(string text)
    {
        isTyping = true;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }
    public void CloseDialog()                                           // ��ȭ ����
    {
        dialogPanel.SetActive(false);
        currentDialog = null;
        StopTypingEffect();             // Ÿ���� ȿ�� ���� �߰�
    }

    // ������ �ʱ�ȭ
    private void ClearChoices()
    {
        foreach(Transform child in choicePanel.transform)
        {
            Destroy(child.gameObject);
        }
        choicePanel.SetActive(false);
    }

    // ������ ���� ó��
    public void SelecChoice(DialogChoiceSO choice)
    {
        if(choice != null && choice.nextId > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialogByld(choice.nextId);
            if(nextDialog != null)
            {
                currentDialog = nextDialog;
                ShowDialog();
            }
            else
            {
                CloseDialog();
            }
        }
        else
        {
            CloseDialog();
        }
    }

    // ������ ǥ��
    private void ShowChoices()
    {
        choicePanel.SetActive(true);

        foreach(var choice in currentDialog.choices)
        {
            GameObject choiceGO = Instantiate(ChoiceButtonPrefab, choicePanel.transform);
            TextMeshProUGUI buttonText = choiceGO.GetComponentInChildren<TextMeshProUGUI>();
            Button button = choiceGO.GetComponent<Button>();

            if(buttonText != null)
            {
                buttonText.text = choice.text;
            }
            if(button != null)
            {
                DialogChoiceSO choiceSO = choice;                           // ���ٽĿ��� ����ϱ� ���ؼ� ���� ������ �Ҵ�
                button.onClick.AddListener(() => SelecChoice(choiceSO));
            }
        }
    }
}
