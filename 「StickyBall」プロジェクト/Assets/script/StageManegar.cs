using System.Collections;
using TMPro;
//using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//AIにコメントを書いてもらいました。

/// <summary>
/// ステージ全体の進行、UI、音量、クリア・ゲームオーバー判定を管理するクラス
/// </summary>
public class StageManegar : MonoBehaviour
{
    // このスクリプトの値を他のスクリプトで参照できるようにするためのシングルトンインスタンス
    public static StageManegar Instance;

    private void Awake()
    {
        // 自身のインスタンスを登録し、他スクリプトから「StageManegar.Instance」でアクセス可能にする
        Instance = this;

        // 1秒間に200回だけ描画（フレーム更新）するように、ターゲットフレームレートを200に固定
        Application.targetFrameRate = 200;
    }

    // ==========================================
    // インスペクターから設定する変数群（シリアライズフィールドなど）
    // ==========================================
    [SerializeField] private int Stage; // 現在のステージ番号

    [SerializeField] private GameObject count; // 画面上に残り操作回数を表示するUIオブジェクト

    [SerializeField] private GameObject clearUI; // ステージクリア時に表示する親UIオブジェクト

    [SerializeField] private GameObject Next; // 次のステージへ進むボタンなどのオブジェクト

    [SerializeField] private GameObject Log; // クリアリザルトなどのログUIオブジェクト

    [SerializeField] private GameObject Setting; // ポーズ（設定）画面のUIオブジェクト

    [SerializeField] private GameObject OverUI; // ゲームオーバー時に表示する親UIオブジェクト

    [SerializeField] private GameObject Over; // ゲームオーバーの演出用オブジェクト（文字など）

    [SerializeField] private GameObject NextOver; // ゲームオーバー後に次のアクションを促すUIオブジェクト

    [SerializeField] private GameObject Check; // リトライなどの確認ダイアログUIオブジェクト

    [SerializeField] private GameObject Black; // 画面フェード用の暗幕オブジェクト
    [SerializeField] private GameObject InviLog; // 連打防止などのための透明な入力遮断用パネルオブジェクト

    [SerializeField] private GameObject BGMint; // BGM音量の数値をテキスト表示するUIオブジェクト
    [SerializeField] private GameObject SEint;  // SE音量の数値をテキスト表示するUIオブジェクト

    [SerializeField] private GameObject SettingMore; // 音量調整などの詳細設定画面UIオブジェクト

    [SerializeField] private GameObject MovesoundOn;  // 移動音「ON」状態を示すUI画像オブジェクト
    [SerializeField] private GameObject MovesoundOff; // 移動音「OFF」状態を示すUI画像オブジェクト

    [SerializeField] private int SetumeiPage = 0; // 現在表示している説明ページのインデックス
    [SerializeField] private bool SetumeiFlag  = false; // 現在表示している説明ページのインデックス

    [SerializeField] private GameObject[] SetumeiPNG; // 説明画像（ページ）を格納する配列
    [SerializeField] private GameObject setumeiLog; // 説明画面の親UIオブジェクト

    // ステージ選択やゲーム中に表示する星（評価）の画像コンポーネント
    [SerializeField] private Image Sterimage1;
    [SerializeField] private Image Sterimage2;
    [SerializeField] private Image Sterimage3;

    // リザルト（クリア画面）で表示する星のオブジェクト
    [SerializeField] private GameObject Sterimage1resurt;
    [SerializeField] private GameObject Sterimage2resurt;
    [SerializeField] private GameObject Sterimage3resurt;

    [SerializeField] private GameObject countresult; // リザルト画面に残り回数を表示するUIオブジェクト

    [SerializeField] private GameObject StartUI; // ステージ開始時の演出用UIオブジェクト
    [SerializeField] private GameObject StartUILogo; // ステージ開始時のタイトルロゴオブジェクト
    [SerializeField] private GameObject SelectCheckUI; // ステージ選択に戻る際の確認ダイアログUIオブジェクト
    [SerializeField] private GameObject Bullball; // 演出の基準となるオブジェクト（青いボール等）
    [SerializeField] private GameObject ParticleFlameStart; // 開始時の炎エフェクトオブジェクト
    [SerializeField] private GameObject FallText; // ボール落下時などに警告表示するテキストオブジェクト
    [SerializeField] private Slider BGMslide; // BGM音量変更用のスライダー
    [SerializeField] private Slider SEslide;  // SE音量変更用のスライダー
    
    public TextMeshProUGUI[] SterTextUI = new TextMeshProUGUI[6];
    
    public Sprite SterFlash; // 獲得済みの星のスプライト（画像）
    public Sprite SterNone;  // 未獲得の星のスプライト（画像）

    public Image BGMImage; // BGMアイコンの画像コンポーネント
    public Image SEImage;  // SEアイコンの画像コンポーネント

    // 音量レベルに応じて切り替えるアイコン用スプライト（大・中・小・ミュート）
    public Sprite BGMs; // 音量：小
    public Sprite BGMm; // 音量：中
    public Sprite BGMl; // 音量：大
    public Sprite BGMz; // 音量：ゼロ（ミュート）

    // アニメーション制御用のコントローラー
    public Animator Sterset; // 星の獲得エフェクト用アニメーター
    public Animator Blackset; // 暗幕のフェードイン・アウト用アニメーター
    public Animator blueflame; // 青い炎のエフェクト用アニメーター

    [SerializeField] int nokori; // 残りの動かせる回数（手手数）

    public int clearred = 0; // ゴールに到達した赤ボールのカウント数

    public int clear; // ステージクリアに必要なボールの合計数

    // 各星（1〜3つ目）を獲得するために必要な「残り回数」のしきい値
    public int sterone;
    public int stersecond;
    public int sterthree;

    // 今回のプレイ、または過去に星を獲得したかどうかのフラグ
    public bool ster1flag;
    public bool ster2flag;
    public bool ster3flag;

    public int stergetCount; // 今回のクリアで最終的に獲得できた星の数

    public int ster; // 保存されている、または現在の星の数

    // 状態管理用のフラグ群（true でその状態であると検知）
    public bool activeClear = false; // クリア演出中・クリア済みフラグ
    public bool activePause = false; // ポーズ（一時停止）画面表示中フラグ
    public bool activeOver = false; // ゲームオーバー演出開始フラグ
    public bool activeGameOver = false; // ゲームオーバーUI表示完了フラグ
    public bool activeFade = false; // 暗幕フェード（リトライ等）処理中フラグ
    public bool activeSetting = false; // 詳細設定画面を開いているかどうかのフラグ
    public bool activemovesound = true; // 移動時に効果音を鳴らすかどうかの設定フラグ

    // Update内での時間経過（フレーム数カウント）に使用するタイマー変数
    private int cleartimer = 0;
    private int overtimer = 0;
    private int fadetimer = 0;

    // 計算用の内部音量変数
    float BGMVol = 0;
    float SEVol = 0;

    // オーディオソース（再生機）と各種効果音・BGMのデータ
    public AudioSource BGM;
    public AudioSource SE;

    public AudioClip BGMSound; // ステージのBGM
    public AudioClip GoalSound; // ボールがゴールに入った時のSE
    public AudioClip ClearSound; // ステージクリア時のSE
    public AudioClip SettingSound; // 設定画面を開いた時のSE
    public AudioClip SettingOutSound; // 設定画面を閉じた時のSE
    public AudioClip smallSound; // 残り回数が少なくなった時などの警告SE
    public AudioClip FallSound; // ゲームオーバー時のSE
    public AudioClip EnterSound; // 決定・決定ボタン入力時のSE
    public AudioClip flashSound; // 星が光った時のSE
    public AudioClip ClapSound; // クリア時の拍手SE
    public AudioClip kannseiSound; // 星3つクリア時の歓声SE
    public AudioClip FallFlashSound; // 開始演出で何かが落下・フラッシュした時のSE
    public AudioClip StartFlashSound; // 開始ロゴ表示時のSE
    public AudioClip titleSound; // ステージタイトル表示時のSE
    public AudioClip FallingSound; // 落下中のループ・演出用SE
    public AudioClip FallRemoveSound; // 落下ペナルティ発生時のSE
    public AudioClip pointsound; // 説明書等のページ切り替え・ポイント移動SE
    public AudioClip setumeiSound; // 説明画面が表示された時のSE


    /// <summary>
    /// ゲーム内でのBGMの音量を調整し、UIに反映して保存する関数
    /// </summary>
    public void SetVolume(float value)
    {
        BGM.volume = value; // オーディオソースの音量を変更

        BGMVol = BGM.volume * 200; // スライダーの値を0〜200の見やすい整数値に変換する

        // 小数点を切り捨ててUIテキストに現在の音量を表示する
        this.BGMint.GetComponent<TextMeshProUGUI>().text = "" + Mathf.FloorToInt(BGMVol);

        // 現在の音量（0〜200）に合わせて音量アイコンの画像（スプライト）を4段階で切り替える
        if (BGMVol > 74)
            BGMImage.sprite = BGMl; // 大
        else if (BGMVol <= 74 && BGMVol > 24)
            BGMImage.sprite = BGMm; // 中
        else if (BGMVol <= 24 && BGMVol > 0)
            BGMImage.sprite = BGMs; // 小
        else if (BGMVol == 0)
            BGMImage.sprite = BGMz; // ミュート

        string B = "BGM";

        // 変更された音量をPlayerPrefsに保存して次回起動時にも維持する
        PlayerPrefs.SetFloat(B, BGM.volume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ゲーム内でのSE（効果音）の音量を調整し、UIに反映して保存する関数
    /// </summary>
    public void SetVolumeSE(float value)
    {
        SE.volume = value; // オーディオソースの音量を変更

        SEVol = SE.volume * 100; // スライダーの値を0〜100の見やすい整数値に変換する

        // 小数点を切り捨ててUIテキストに現在の音量を表示する
        this.SEint.GetComponent<TextMeshProUGUI>().text = "" + Mathf.FloorToInt(SEVol);

        // 現在の音量（0〜100）に合わせて効果音アイコンの画像（スプライト）を4段階で切り替える
        if (SEVol > 74)
            SEImage.sprite = BGMl; // 大
        else if (SEVol <= 74 && SEVol > 24)
            SEImage.sprite = BGMm; // 中
        else if (SEVol <= 24 && SEVol > 0)
            SEImage.sprite = BGMs; // 小
        else if (SEVol == 0)
            SEImage.sprite = BGMz; // ミュート

        string S = "SE";

        // 変更された効果音音量をPlayerPrefsに保存して次回起動時にも維持する
        PlayerPrefs.SetFloat(S, SE.volume);
        PlayerPrefs.Save();
    }

    // Start関数はMonoBehaviourが作成された後、最初のUpdateが実行される前に一度だけ呼ばれる
    void Start()
    {
        // 初期状態の各フラグを設定（開始時は操作できないようポーズや設定をtrueにしておく）
        activeClear = false;
        activePause = true;
        activeOver = false;
        activeGameOver = false;
        activeFade = false;
        activeSetting = true;
        activemovesound = true;

        string key = "stage_" + Stage + "_sters";

        // 過去に獲得したこのステージの星の数をロードする
        var SterStartSet = LoadStageData(key);
        ster = PlayerPrefs.GetInt(key, 0);
        stergetCount = 0; // 今回獲得した星のカウントをリセット

        SterTextUI[0].text = $"{sterone}";
        SterTextUI[1].text = $"{sterone}";
        SterTextUI[2].text = $"{stersecond}";
        SterTextUI[3].text = $"{stersecond}";
        SterTextUI[4].text = $"{sterthree}";
        SterTextUI[5].text = $"{sterthree}";
        
        // ロードした値に応じて、過去に星1〜3を達成しているかフラグを割り振る
        if (SterStartSet >= 1)
            ster1flag = true;
        else
            ster1flag = false;

        if (SterStartSet >= 2)
            ster2flag = true;
        else
            ster2flag = false;

        if (SterStartSet == 3)
            ster3flag = true;
        else
            ster3flag = false;

        // 星1の獲得状況に応じて、画面上の星の見た目（点灯・消灯）を切り替える
        if (ster1flag)
            Sterimage1.sprite = SterFlash;
        else
            Sterimage1.sprite = SterNone;

        // 星2の獲得状況に応じて切り替え
        if (ster2flag)
            Sterimage2.sprite = SterFlash;
        else
            Sterimage2.sprite = SterNone;

        // 星3の獲得状況に応じて切り替え
        if (ster3flag)
            Sterimage3.sprite = SterFlash;
        else
            Sterimage3.sprite = SterNone;

        // 各種タイマー変数を一律リセット
        overtimer = 0;
        cleartimer = 0;
        fadetimer = 0;

        // ゲーム開始時に不要なUIオブジェクトを一旦非表示（アクティブを解除）にする
        clearUI.SetActive(false); // クリアUI非表示
        OverUI.SetActive(false);  // ゲームオーバーUI非表示
        StartUI.SetActive(false); // スタート演出UI非表示

        string B = "BGM";
        string S = "SE";

        // 保存されている音量データをロード（データがない場合はデフォルト値 BGM:0.2f, SE:0.5f）
        BGM.volume = PlayerPrefs.GetFloat(B, 0.2f);
        SE.volume = PlayerPrefs.GetFloat(S, 0.5f);

        // 移動時の効果音設定をロード（1ならON、0ならOFF）
        var soundMove = PlayerPrefs.GetInt("MoveSound", 1);
        if (soundMove == 1)
            activemovesound = true;
        else
            activemovesound = false;

        // スライダーの初期位置をロードしたBGM音量に合わせる
        BGMslide.value = BGM.volume;
        BGMVol = BGM.volume * 200; // テキスト表示用に変換
        this.BGMint.GetComponent<TextMeshProUGUI>().text = "" + Mathf.FloorToInt(BGMVol); // 音量テキスト更新

        // ロードしたBGM音量に合わせてアイコン画像を決定
        if (BGMVol > 74)
            BGMImage.sprite = BGMl;
        else if (BGMVol <= 74 && BGMVol > 24)
            BGMImage.sprite = BGMm;
        else if (BGMVol <= 24 && BGMVol > 0)
            BGMImage.sprite = BGMs;
        else if (BGMVol == 0)
            BGMImage.sprite = BGMz;

        // スライダーの初期位置をロードしたSE音量に合わせる
        SEslide.value = SE.volume;
        SEVol = SE.volume * 100; // テキスト表示用に変換
        this.SEint.GetComponent<TextMeshProUGUI>().text = "" + Mathf.FloorToInt(SEVol); // 音量テキスト更新

        // ロードしたSE音量に合わせてアイコン画像を決定
        if (SEVol > 74)
            SEImage.sprite = BGMl;
        else if (SEVol <= 74 && SEVol > 24)
            SEImage.sprite = BGMm;
        else if (SEVol <= 24 && SEVol > 0)
            SEImage.sprite = BGMs;
        else if (SEVol == 0)
            SEImage.sprite = BGMz;

        // 暗幕（Black）の初期位置を画面外に大きくずらして配置する調整
        Black.SetActive(true);

        // このステージが解放（プレイ開始）されたという進捗度フラグを保存
        PlayerPrefs.SetInt("OpenCheck_" + Stage, 1);

        // ステージ開始のアニメーション・演出コルーチンを実行
        StartCoroutine(StageStart());
    }

    /// <summary>
    /// PlayerPrefsから特定のキーの整数データを読み込む。データがなければ 0 を返す。
    /// </summary>
    public int LoadStageData(string key)
    {
        return PlayerPrefs.GetInt(key, 0);
    }

    /// <summary>
    /// ステージ開始時の演出を制御するコルーチン
    /// </summary>
    public IEnumerator StageStart()
    {
       

        // 暗幕をフェードアウト（画面を見せる）するアニメーションのトリガーを実行
        Blackset.SetTrigger("Fadeout;");
        
        yield return new WaitForSeconds(1.5f);

        // 青ボールの位置を基準にして、開始時の炎エフェクトの初期座標を計算・配置する
        Vector3 bluedayo = Bullball.transform.position;
        Vector3 bluestart = new Vector3(bluedayo.x - 0.1f, bluedayo.y + 12.0f, bluedayo.z + 0.08f);
        ParticleFlameStart.transform.position = bluestart;
        Vector3 blueparticle = ParticleFlameStart.transform.position;

        // 暗幕の位置を元の位置に戻して非アクティブ化
        Black.SetActive(false);

        // スタートUIの座標を画面内に戻してアクティブ化
        StartUI.SetActive(true);

        // タイトル表示音を再生
        SE.PlayOneShot(titleSound);

        yield return new WaitForSeconds(3.0f);

        // スタートUIの役割が終わったため座標を画面外に逃がす
        StartUI.SetActive(false);

        // 炎が落下するアニメーションのトリガーを実行し、落下音を再生
        blueflame.SetTrigger("flamefoll");
        SE.PlayOneShot(FallFlashSound);

        // 500フレームの間、炎エフェクトを毎フレーム少しずつ下方向に移動させる演出
        for (int i = 0; i < 500; i++)
        {
            if (blueparticle.y > bluedayo.y)
            {
                blueparticle.y -= 0.03f;
                ParticleFlameStart.transform.position = blueparticle;
            }
            yield return null;
        }

        // ステージ1、の初回開始時のみ、説明書（チュートリアル）画面を表示する処理
        if (Stage == 1)
        {
            var setumeiCheck1 = PlayerPrefs.GetInt("stagesetumei1", 0);
            if (setumeiCheck1 == 0)
            {
                StartCoroutine(SetumeiSistem()); // 説明書表示コルーチン開始
            }
            else
            {
                // 既に説明書を読んでいる場合は、ポーズと設定を解除してゲームを開始させる
                activePause = false;
                activeSetting = false;
            }
        }
        else
        {
            // 説明書がないステージはそのままプレイ可能状態にする
            activePause = false;
            activeSetting = false;
        }

        // ゲーム開始を告げるフラッシュ音を鳴らし、スタートロゴを表示
        SE.PlayOneShot(StartFlashSound);
        StartUILogo.SetActive(true);

        // ループ設定でメインBGMの再生を開始
        this.BGM.loop = true;
        this.BGM.clip = BGMSound;
        this.BGM.Play();

        yield return new WaitForSeconds(1.0f);
        
        // 開始ロゴやエフェクト、不要なログ用パネルを非表示にして完全にゲームプレイ状態へ移行
        StartUILogo.SetActive(false);
        ParticleFlameStart.SetActive(false);
        InviLog.SetActive(false);
    }

    /// <summary>
    /// 残り動かせる回数を1減らし、UIテキストの色や数値を更新する関数
    /// </summary>
    public void movedown()
    {
        nokori--; // 残り回数を減算

        // 残り回数がちょうど10回になったら警告用の効果音を1回鳴らす
        if (nokori == 10)
        {
            this.SE.PlayOneShot(smallSound);
        }

        // 画面のカウントテキストを更新
        TextMeshProUGUI textMesh = this.count.GetComponent<TextMeshProUGUI>();
        this.count.GetComponent<TextMeshProUGUI>().text = "" + nokori;

        // 残り回数が10以下なら赤色、11以上なら指定の淡い水色（ほぼ白色）にテキスト色を切り替える
        if (nokori <= 10)
        {
            textMesh.color = Color.red;
        }
        else
        {
            textMesh.color = new Color(230f / 255f, 253f / 255f, 255f / 255f);
        }
    }

    /// <summary>
    /// 赤ボールがゴールに入った時に他スクリプトから呼び出されるカウントアップ関数
    /// </summary>
    public void redin()
    {
        clearred++; // ゴールに入った赤ボールの数を加算

        this.SE.PlayOneShot(GoalSound); // ゴール効果音を再生
    }

    /// <summary>
    /// ポーズ（一時停止）ボタンを押したときに設定画面をスライド表示する関数
    /// </summary>
    public void PAUSE()
    {
        this.SE.PlayOneShot(SettingSound); // メニュー開閉音を再生
        Setting.SetActive(true);
        activePause = true; // ポーズ中フラグを立ててゲーム内の動作を制限させる
    }

    /// <summary>
    /// ポーズ（一時停止）画面で「戻る」を押したときに画面を戻す関数
    /// </summary>
    public void PAUSEoff()
    {
        this.SE.PlayOneShot(SettingOutSound); // メニュー閉じる音を再生
        this.Setting.SetActive(false);
        activePause = false; // ポーズ中フラグを解除
    }

    /// <summary>
    /// 残り手数がゼロになるなどの敗北条件を満たしたときに演出を開始する関数
    /// </summary>
    public void GameOver()
    {
        this.Over.SetActive(true); // ゲームオーバー演出オブジェクトを表示
        this.BGM.Stop(); // 流れていたBGMを停止
        this.SE.PlayOneShot(FallSound); // 敗北・落下時の効果音を再生
        StartCoroutine(OnUIOver());
    }

    public IEnumerator OnUIOver()
    {
        yield return new WaitForSeconds(1.0f);
        this.NextOver.SetActive(true);
    }

    /// <summary>
    /// ステージを最初からやり直す（リトライ）ボタンを押した時に呼ばれる関数
    /// </summary>
    public void Retry()
    {
        // まだ画面フェード（暗転）処理中でなければ実行
        if (!activeFade)
        {
            activeFade = true; // 暗幕フェード中フラグをON
            InviLog.SetActive(true); // リトライ連打防止のため透明パネルで入力を遮断
            this.SE.PlayOneShot(EnterSound); // 決定音を再生
            this.Black.SetActive(true); // 暗幕オブジェクトをアクティブ化

            // 暗幕の座標をフェードインアニメーションの初期位置へとスライド移動
            this.Black.SetActive(true);

            Blackset.SetTrigger("Fadein"); // 暗幕が画面を覆うフェードインアニメーションを実行
        }
    }

    /// <summary>
    /// リザルト画面などで次のステージに進むボタンを押した時に呼ばれる関数
    /// </summary>
    public void NextStage()
    {
        InviLog.SetActive(true); // ボタン連打防止パネルを表示
        this.SE.PlayOneShot(EnterSound); // 決定音を再生
        this.Black.SetActive(true); // 暗幕オブジェクトをアクティブ化

        // 暗幕の座標をアニメーション初期位置へ移動
        this.Black.SetActive(true);

        Blackset.SetTrigger("Fadein"); // フェードインアニメーションを実行

        // このステージで最終的に確定した星の数をPlayerPrefsに保存
        PlayerPrefs.SetInt("SterCheck_" + Stage, ster);
        PlayerPrefs.Save();

        // 実際にシーンをロードするコルーチンを実行
        StartCoroutine(StageGo());
    }

    /// <summary>
    /// フェードアニメーションを待ってから次のステージシーンをロードするコルーチン
    /// </summary>
    public IEnumerator StageGo()
    {
        yield return new WaitForSeconds(4.0f);

        // Stage変数（整数型型）の数値をそのままビルドインデックスとして次のシーンを読み込む
        SceneManager.LoadScene(Stage);
    }

    /// <summary>
    /// ポーズメニュー等から「ステージ選択に戻る」を押した際の確認ダイアログ表示関数
    /// </summary>
    public void selectCheck()
    {
        SE.PlayOneShot(smallSound); // 選択音を再生
        this.SelectCheckUI.SetActive(true);
    }

    /// <summary>
    /// 「ステージ選択に戻らない（キャンセル）」を押した際の確認ダイアログ非表示関数
    /// </summary>
    public void selectCheckback()
    {
        SE.PlayOneShot(SettingOutSound); // キャンセル音を再生
        this.SelectCheckUI.SetActive(false);// 確認ダイアログを画面外へスライドして戻す
    }

    /// <summary>
    /// 確認ダイアログで「はい（戻る）」を選んだ時に実行され、フェードを挟んで遷移する関数
    /// </summary>
    public void selectStageGo()
    {
        InviLog.SetActive(true); // 連打防止パネルを表示
        this.SE.PlayOneShot(EnterSound); // 決定音を再生
        this.Black.SetActive(true); // 暗幕をアクティブ化

        // 暗幕の座標をスライドしてフェード準備
        this.Black.SetActive(true);

        Blackset.SetTrigger("Fadein"); // 暗幕フェードイン開始
        StartCoroutine(selectGo()); // シーン終了（ゲーム終了）コルーチン実行
    }

    /// <summary>
    /// フェード完了を待ってエディタの再生を止めるか、ゲームアプリを終了するコルーチン
    /// </summary>
    public IEnumerator selectGo()
    {
        for (int i = 0; i < 500; i++) // 500フレーム待機
        {
            yield return null;
        }

#if UNITY_EDITOR
        // Unityエディタ上での実行中なら、エディタの再生モードをオフにする
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 実機（ビルドされたゲームアプリ）なら、アプリケーション自体を終了（クイット）する
        Application.Quit();
#endif
    }

    /// <summary>
    /// ポーズ画面などで「やり直す（リトライ）」の確認ダイアログを表示する関数
    /// </summary>
    public void kakunin()
    {
        this.Check.SetActive(true);// 確認ダイアログUIを画面内に移動
        this.SE.PlayOneShot(smallSound); // 効果音再生
    }

    /// <summary>
    /// リトライの確認ダイアログで「いいえ（キャンセル）」を選んだ時に実行する関数
    /// </summary>
    public void kakuninOff()
    {
        this.Check.SetActive(false);// 確認ダイアログUIを画面外に引き戻す
        this.SE.PlayOneShot(SettingOutSound); // キャンセル音再生
    }

    /// <summary>
    /// ポーズメニュー内から、さらに詳細な音量調整画面（詳細設定）を開く関数
    /// </summary>
    public void SettingMoreOpen()
    {
        activeSetting = true; // 詳細設定を操作中フラグをON

        this.SettingMore.SetActive(true);// 詳細設定UIをY軸方向にスライドして表示

        this.SE.PlayOneShot(SettingSound); // メニュー表示音を再生
    }

    /// <summary>
    /// スライダー移動時などに、ポイントを指す効果音を再生する関数
    /// </summary>
    public void pointSound()
    {
        SE.PlayOneShot(pointsound);
    }

    /// <summary>
    /// 詳細設定画面を閉じて、通常のポーズメニューに戻る関数
    /// </summary>
    public void SettingMoreClose()
    {
        activeSetting = false; // 詳細設定操作中フラグをOFF

        this.SettingMore.SetActive(false); // 詳細設定UIを画面外へスライドして戻す

        this.SE.PlayOneShot(SettingOutSound); // メニュー閉じる音を再生
    }

    /// <summary>
    /// スライダーから指を離した（ポイントアップした）際にSEの音量確認として再生される関数
    /// </summary>
    public void OnPointUp()
    {
        this.SE.PlayOneShot(EnterSound);
    }

    /// <summary>
    /// オプションで「移動時に効果音を鳴らさない」設定にした際に呼び出される関数
    /// </summary>
    public void CutmoveSound()
    {
        activemovesound = false; // 移動音フラグをOFF
        this.SE.PlayOneShot(EnterSound); // 設定変更の決定音を再生
        PlayerPrefs.SetInt("MoveSound", 0); // 設定値を「0（無効）」として保存
        PlayerPrefs.Save();
    }

    /// <summary>
    /// オプションで「移動時に効果音を鳴らす」設定にした際に呼び出される関数
    /// </summary>
    public void PutmoveSound()
    {
        activemovesound = true; // 移動音フラグをON
        this.SE.PlayOneShot(EnterSound); // 設定変更の決定音を再生
        PlayerPrefs.SetInt("MoveSound", 1); // 設定値を「1（有効）」として保存
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ステージクリアの条件を完全に満たした（ゴール数が規定値に達した）時のリザルト演出コルーチン
    /// </summary>
    public IEnumerator StageClear()
    {
        this.BGM.PlayOneShot(ClearSound); // クリア用効果音を1回再生
        activeClear = true; // クリアした状態の判定フラグをONにする
        this.clearUI.SetActive(true); // クリアUI（背景など）を表示状態にする
        this.Log.SetActive(false); // 通常プレイ時のゲームログ等のUIを非表示にする
        this.countresult.GetComponent<TextMeshProUGUI>().text = "" + nokori; // 残り回数の最終結果をリザルトUIにテキスト表示

        // もしまだ1つ目の星を獲得していなければ、リザルト画面の星1表示オブジェクトを一旦消しておく（後でアニメ演出するため）
        if (!ster1flag)
            this.Sterimage1resurt.SetActive(false);

        // もしまだ2つ目の星を獲得していなければ、リザルト画面の星2表示オブジェクトを一旦消しておく
        if (!ster2flag)
            this.Sterimage2resurt.SetActive(false);

        // もしまだ3つ目の星を獲得していなければ、リザルト画面の星3表示オブジェクトを一旦消しておく
        if (!ster3flag)
            this.Sterimage3resurt.SetActive(false);

        this.clearUI.SetActive(true); // クリア表示UI一式を画面外からスライドさせて定位置へ移動

        yield return new WaitForSeconds(1.5f);

        // 【星1つの獲得判定】残り回数がしきい値「sterone」以上、かつ今回まだ星1を獲得していない場合
        if (sterone <= nokori && !ster1flag)
        {
            ster1flag = true; // 星1を獲得状態にする
            Sterset.SetTrigger("FlashTrriger1"); // 星1が輝いて出現するアニメーションを実行
            Sterimage1resurt.SetActive(true); // リザルトの星1オブジェクトを表示
            stergetCount = 1; // 獲得数を1に更新

            yield return new WaitForSeconds(0.4f);

            SE.PlayOneShot(flashSound); // 星がキラめく効果音を再生

            yield return new WaitForSeconds(0.4f);
        }

        // 【星2つの獲得判定】過去に星2をまだ獲得していない場合のみ判定
        if (!ster2flag)
        {
            // 残り回数がしきい値「stersecond」以上なら獲得
            if (stersecond <= nokori)
            {
                ster2flag = true; // 星2を獲得状態にする
                Sterset.SetTrigger("FlashTrriger2"); // 星2出現アニメーション実行
                Sterimage2resurt.SetActive(true); // リザルトの星2オブジェクトを表示
                stergetCount = 2; // 獲得数を2に更新

                yield return new WaitForSeconds(0.4f);

                SE.pitch = 1.2f; // 音のピッチ（高低）を少し高くして盛り上がりを演出
                SE.PlayOneShot(flashSound); // キラめく効果音を再生

                yield return new WaitForSeconds(0.4f);
            }
        }

        // 【星3つの獲得判定】残り回数が最高しきい値「sterthree」以上、かつ今回まだ星3を獲得していない場合
        if (sterthree <= nokori && !ster3flag)
        {
            ster3flag = true; // 星3を獲得状態にする（パーフェクトクリア）
            Sterset.SetTrigger("FlashTrriger3"); // 星3出現アニメーション実行
            Sterimage3resurt.SetActive(true); // リザルトの星3オブジェクトを表示
            stergetCount = 3; // 獲得数を3に更新

            yield return new WaitForSeconds(0.4f);

            SE.pitch = 1.5f; // 音のピッチをさらに高くして最高評価の演出にする
            SE.PlayOneShot(flashSound); // キラめく効果音を再生

            yield return new WaitForSeconds(0.4f);
        }

        // 今回新しく星を獲得できており、かつ記録が更新（または同じ数を初獲得）されている場合の演出
        if (stergetCount > 0 && ster != stergetCount)
        {
            yield return new WaitForSeconds(0.2f);

            SE.pitch = 1; // 変更していたSEのピッチを通常（1倍速）に戻す

            // もし最高の星3つを獲得していたら、大歓声の効果音を重ねて再生する
            if (stergetCount == 3)
            {
                SE.PlayOneShot(kannseiSound);
            }

            SE.PlayOneShot(ClapSound); // 祝福の拍手喝采SEを再生

            yield return new WaitForSeconds(1.5f);

            ster = stergetCount; // 現在の星の数（内部記録）を更新
        }

        // 最終的なステージクリア状況・スコアをPlayerPrefsに永続セーブする
        string key = "stage_" + Stage + "_sters";
        PlayerPrefs.SetInt(key, ster); // 星の数を保存

        string key2 = "stage_" + Stage + "_nokori";
        // 今回の残り回数が、これまでの自己ベスト（ハイスコア）を上回っていればデータを上書き保存
        if (nokori > this.LoadStageData(key2))
            PlayerPrefs.SetInt(key2, nokori);

        PlayerPrefs.SetInt("StageClear", 1); // 該当ステージをクリアしたという証明フラグを1に
        PlayerPrefs.SetInt("Stage", Stage); // 最後にクリアしたステージ番号を記録
        PlayerPrefs.Save(); // ディスクへ書き込みを確定

        Log.SetActive(true); // 次のステージに進むボタンや選択肢UIを画面に表示
    }

    /// <summary>
    /// 残り手数が切れるなどしてゲームオーバー状態になった直後に、少しの間を経てUIを表示するコルーチン
    /// </summary>
    public IEnumerator OverStart()
    {
        // プレイヤーがこれ以上操作できないよう、ポーズと設定フラグを強制的にONにする
        activePause = true;
        activeSetting = true;

        yield return new WaitForSeconds(1.0f);

        // まだゲームオーバー画面に遷移しておらず、かつクリア条件を同時に満たしていない場合のみ実行
        if (!activeOver && clearred != clear)
        {
            this.OverUI.SetActive(true); // ゲームオーバー用UIのアクティブ化
            this.Over.SetActive(false);  // 演出用文字などは一旦オフに
            activeOver = true; // ゲームオーバー演出中フラグをONにする
        }
    }

    /// <summary>
    /// ゲーム開始時のチュートリアル（説明画面）表示を制御するコルーチン
    /// </summary>
    public IEnumerator SetumeiSistem()
    {
        yield return new WaitForSeconds(1.0f);
        SetumeiFlag = true;
        SetumeiPage = 0;

        for (int i = 0; i < 7; i++)
        {
            SetumeiPNG[i].SetActive(false);
        }

        SetumeiPNG[0].SetActive(true);

        activePause = true;
        activeSetting = true;

        // 説明ダイアログUIを、中央画面内にスライド移動させる
        this.setumeiLog.SetActive(true);
        SE.PlayOneShot(setumeiSound); // 説明画面出現音を再生
    }

    /// <summary>
    /// 説明書の「次へ」ボタンを押したときにページを進める関数
    /// </summary>
    public void NextSetumei()
    {
        SetumeiPNG[SetumeiPage].SetActive(false); // 現在表示している説明画像を非表示にする
        SetumeiPage++; // ページインデックスを1進める
        SetumeiPNG[SetumeiPage].SetActive(true);  // 新しいページの説明画像を表示する
        SE.PlayOneShot(pointsound); // ページめくり音を再生
    }

    /// <summary>
    /// 説明書の「前へ」ボタンを押したときにページを戻す関数
    /// </summary>
    public void BackSetumei()
    {
        SetumeiPNG[SetumeiPage].SetActive(false); // 現在表示している説明画像を非表示にする
        SetumeiPage--; // ページインデックスを1戻す
        SetumeiPNG[SetumeiPage].SetActive(true);  // 前のページの説明画像を表示する
        SE.PlayOneShot(pointsound); // ページ戻り音を再生
    }

    /// <summary>
    /// 説明書の「閉じる（終了）」ボタンを押したときに画面を片付ける関数
    /// </summary>
    public void SetumeiEnd()
    {
        // 説明ダイアログUIを画面外にスライド移動させて片付ける
        this.setumeiLog.SetActive(false);
        SE.PlayOneShot(SettingOutSound); // メニューを閉じる効果音を再生
        SetumeiFlag=false;

        // 現在プレイしているステージ（1番か6番か）に合わせて、「既読フラグ（1）」を書き込んで次回からスキップ可能にする
        if (Stage == 1)
        {
            PlayerPrefs.SetInt("stagesetumei1", 1);
        }

        // ゲームをプレイできるように、一時停止状態と設定フラグを解除して操作を解放する
        activePause = false;
        activeSetting = false;
        PlayerPrefs.Save(); // 既読データを保存
    }


    // Update関数はフレーム毎に1回呼び出される、ゲームのメインロジックの監視場所
    void Update()
    {
        // オプションの「移動時に音を出すかどうか」のフラグ状態を見て、ON/OFFのUI画像の表示を毎フレーム同期する
        if (activemovesound)
        {
            MovesoundOn.SetActive(true);
            MovesoundOff.SetActive(false);
        }
        else
        {
            MovesoundOn.SetActive(false);
            MovesoundOff.SetActive(true);
        }

        if(Input.GetKeyDown(KeyCode.G) && !SetumeiFlag)
        {
            StartCoroutine(SetumeiSistem());
        }

        // 【キー入力監視】ゲーム中にEscキーが押され、かつ「ゲームオーバー中ではない」「クリア中ではない」「フェード中ではない」「詳細設定を開いていない」場合
        if (Input.GetKeyDown(KeyCode.Escape) && !activeOver && !activeClear && !activeFade && !activeSetting)
        {
            // ポーズ中でなければポーズ画面を開く
            if (!activePause)
                PAUSE();
            else
            {
                // 既にポーズ中であればポーズ画面を閉じ、各種確認中だったダイアログUIの座標を強制的に初期位置（画面外左上など）へリセットする
                PAUSEoff();
                this.Check.SetActive(false);
                this.SelectCheckUI.SetActive(false);
            }
        }

        // 【クリア判定監視】集めたボールがクリア目標数に達しており、かつクリアタイマーが上限（1000）に達していない場合
        if (clearred == clear && cleartimer <= 1000)
        {
            // クリア成立の瞬間（最初の100フレーム未満の間）、流れていたステージBGMを即座に停止する
            if (cleartimer < 100)
                this.BGM.Stop();

            cleartimer++; // クリア用待機フレームタイマーをカウントアップ

            // 175フレームが経過し、まだクリア演出（StageClearコルーチン）が走っていなければ実行
            if (cleartimer >= 175 && !activeClear)
            {
                StartCoroutine(StageClear()); // クリア演出・ハイスコア保存処理を開始
                cleartimer = 1000; // タイマーを一気に1000へ飛ばし、このif文が重複して何度も実行されるのを防ぐ（多重実行防止）
            }
        }

        // 【ゲームオーバー手数切れ監視】残り回数が0以下になり、かつまだゲームオーバー演出中でなく、クリア数も未達の場合
        if (nokori <= 0 && !activeOver && clearred != clear)
        {
            StartCoroutine(OverStart()); // ゲームオーバー演出開始（UIスライドなど）コルーチンを実行
        }

        // 【ゲームオーバー時BGM失速演出】ゲームオーバー演出が開始され、かつBGMのピッチ（再生速度・高さ）が-1より大きい（まだ下がっている途中）場合
        if (activeOver && this.BGM.pitch > -1)
        {
            // BGMの再生速度が0（完全停止状態）以下になり、まだ本当のゲームオーバーUIが出現していなければ
            if (this.BGM.pitch <= 0 && !activeGameOver)
            {
                this.activeGameOver = true; // ゲームオーバーUI表示完了フラグをON
                GameOver(); // ゲームオーバーの文字や演出パーツを表示する本処理関数を叩く
            }
            else
            {
                // BGMのピッチを毎フレーム「0.003」ずつ削り、音が間延びしながら低くなって止まる「テープストップ効果」を再現
                this.BGM.pitch -= 0.003f;
            }
        }

        // 【ゲームオーバー後の選択肢表示監視】ゲームオーバーUIが出揃い、overtimerが上限（1000）に達していない場合
        if (activeGameOver && overtimer < 1000)
        {
            overtimer++; // ゲームオーバー後タイマーをカウントアップ

            // 200フレーム経過したら、次のリトライボタンなどを画面下からスライド移動させて選択可能状態にする
            if (overtimer > 200 && overtimer < 1000)
            {
                
                overtimer = 1010; // タイマーを上限値以上に設定し、重複処理を防止
            }
        }

        // 【暗幕リトライフェード監視】リトライボタン等が押されて「activeFade」フラグが立っている場合
        if (activeFade)
        {
            // 暗幕が画面を完全に覆う時間（500フレーム）が経過したら
            if (this.fadetimer >= 500)
            {
                this.BGM.Stop(); // BGMを完全にストップ
                string Restart = SceneManager.GetActiveScene().name; // 現在開いているアクティブなシーン（ステージ）の名前を取得
                SceneManager.LoadScene(Restart); // 同じシーンを最初からロードし直すことで、ステージを完全にリセット（リトライ完了）
            }
            else
            {
                // 500フレームに達するまでの間、BGMの音量を毎フレーム「0.0005」ずつ徐々に絞る（オーディオのフェードアウト処理）
                if (this.BGM.volume > 0)
                    this.BGM.volume -= 0.0005f;
                fadetimer++; // フェードタイマーをカウントアップ
            }
        }
    }
}