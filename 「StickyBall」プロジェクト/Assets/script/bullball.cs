using UnityEngine;

// AIによってコメントを書いてもらいました。
public class bullball : MonoBehaviour
{
    // ==========================================
    // メンバ変数定義
    // ==========================================
    GameObject Maneger;      // ステージ管理オブジェクト（StageManegar）の参照
    GameObject Change;       // 位置を入れ替える対象オブジェクト（Change boll）の参照

    bool away;               // 自ボールが進行方向に移動可能かどうかのフラグ
    bool freeze;             // 一時停止やクリア時など、操作を無効化するフラグ
    bool move;               // 同時押し防止用：キーが押されている間、追加の入力を制限するフラグ
    bool movesound;          // 移動時に効果音を鳴らすかどうかの内部フラグ

    [Header("Audio Settings")]
    public AudioSource sound;         // 効果音再生用のオーディオソース
    public AudioClip MoveSound;       // 移動時のSE
    public AudioClip RotateSound;     // 位置入れ替え時のSE


    // ==========================================
    // 初期化処理
    // ==========================================
    void Start()
    {
        away = true;
        move = false;
        freeze = false; // 初期状態は操作可能

        // シーン内から必要な管理オブジェクトを検索して取得
        this.Change = GameObject.Find("Change boll");
        this.Maneger = GameObject.Find("StageManegar");
    }

    // 青いボールで動かす関数
    public void StickyBall(Vector3 MoveFront)
    {

        move = true; // 入力ロック
        away = true; // 移動可能フラグを初期化

        Vector3 origin = transform.position; // 移動前の現在位置を退避
        var F = Vector3.forward;
        var R = Vector3.right;

        var XMove = 0;
        var ZMove = 0;

        Vector3 MoveSide = R;
        float distance = 1.0f;               // 隣のマスまでの距離

        if (MoveFront == F || MoveFront == -F)
        {
            MoveSide = R;
            if (MoveFront == F)
            {
                ZMove = 1;
            }
            else if (MoveFront == -F)
            {
                ZMove = -1;
            }
        }
        else if (MoveFront == R || MoveFront == -R)
        {
            MoveSide = F;
            if (MoveFront == R)
            {
                XMove = 1;
            }
            else if (MoveFront == -R)
            {
                XMove = -1;
            }
        }

        // 進行方向にレイを飛ばし、隣のマスにあるオブジェクトを検知
        if (Physics.Raycast(origin, MoveFront, out RaycastHit hit, distance))
        {
            // Case A-1: 隣が「white」ボールの場合（押し出し判定）
            if (hit.collider.CompareTag("white"))
            {
                GameObject target = hit.collider.gameObject;
                Vector3 nyan = target.transform.position;

                // whiteボールのさらに先（進行方向）に障害物があるかチェック
                if (Physics.Raycast(nyan, MoveFront, out RaycastHit what, distance))
                {
                    away = false; // 先が詰まっているので移動不可
                }
                else
                {
                    // 先が空いているのでwhiteボールを進行方向に1マス押し出す
                    target.transform.position += new Vector3(XMove, 0, ZMove);

                    // 【引っ張り判定】自マスの背後（-MoveFront方向）にボールがあるかチェック
                    if (Physics.Raycast(origin, -MoveFront, out RaycastHit hit2, distance))
                    {
                        // 背後に「white」または「red」があれば、連動して進行方向に1マス引っ張る
                        if (hit2.collider.CompareTag("white") || hit2.collider.CompareTag("red"))
                        {
                            GameObject pull = hit2.collider.gameObject;
                            pull.transform.position += new Vector3(XMove, 0, ZMove);
                        }
                    }
                }
            }
            // Case A-2: 隣が「red」ボールの場合（ゴール判定付き押し出し）
            else if (hit.collider.CompareTag("red"))
            {
                GameObject target = hit.collider.gameObject;
                Vector3 nyan = target.transform.position;

                // redボールのさらに先（進行方向）をチェック
                if (Physics.Raycast(nyan, MoveFront, out RaycastHit what, distance))
                {
                    // redボールの先が「goal」タグの場合のみ押し出し可能
                    if (what.collider.CompareTag("goal"))
                    {
                        target.transform.position += new Vector3(XMove, 0, ZMove); // redをゴールへ押し出す

                        // 【引っ張り判定】背後のボールを連動して進行方向へ移動
                        if (Physics.Raycast(origin, -MoveFront, out RaycastHit hit2, distance))
                        {
                            if (hit2.collider.CompareTag("white") || hit2.collider.CompareTag("red"))
                            {
                                GameObject pull = hit2.collider.gameObject;
                                pull.transform.position += new Vector3(XMove, 0, ZMove);
                            }
                        }
                    }
                    else
                    {
                        away = false; // ゴール以外の壁や障害物なら移動不可
                    }
                }
                else
                {
                    // 先が完全に空いているなら通常通りredを進行方向に押し出す
                    target.transform.position += new Vector3(XMove, 0, ZMove);

                    // 【引っ張り判定】背後のボールを連動して進行方向へ移動
                    if (Physics.Raycast(origin, -MoveFront, out RaycastHit hit2, distance))
                    {
                        if (hit2.collider.CompareTag("white") || hit2.collider.CompareTag("red"))
                        {
                            GameObject pull = hit2.collider.gameObject;
                            pull.transform.position += new Vector3(XMove, 0, ZMove);
                        }
                    }
                }
            }
            // Case A-3: 隣が指定タグ（white/red）以外（壁など）の場合
            else
            {
                away = false; // 移動不可
            }
        }
        // Case A-4: 進行方向に何も障害物がない場合
        else
        {
            // 【引っ張り判定】進行方向が空でも、背後にボールがあれば連動して進行方向に引っ張る
            if (Physics.Raycast(origin, -MoveFront, out RaycastHit hit2, distance))
            {
                if (hit2.collider.CompareTag("white") || hit2.collider.CompareTag("red"))
                {
                    GameObject pull = hit2.collider.gameObject;
                    pull.transform.position += new Vector3(XMove, 0, ZMove);
                }
            }
        }

        // ------------------------------------------
        // 【巻き込み移動判定（各移動キー時：両側面のチェック）】
        // 自ボールが移動できる場合、側面に隣接するボールも連動して進行方向へスライドさせる
        // ------------------------------------------
        if (away)
        {

            // 自マスの側面1（MoveSide方向）にボールがいるかチェック
            if (Physics.Raycast(origin, MoveSide, out RaycastHit hit3, distance))
            {
                if (hit3.collider.CompareTag("white") || hit3.collider.CompareTag("red"))
                {
                    GameObject target = hit3.collider.gameObject;
                    Vector3 nyan = target.transform.position;

                    // 【注意】現状のコードではレイ検出時の処理（ifブロック内）が空のため、
                    // その側面ボールの進行方向に「何か障害物がある場合」に、進行方向へ移動させる挙動になっています
                    if (Physics.Raycast(nyan, MoveFront, out RaycastHit what, distance)) { }
                    else
                    {
                        target.transform.position += new Vector3(XMove, 0, ZMove);
                    }
                }
            }

            // 自マスの側面2（-MoveSide方向）にボールがいるかチェック
            if (Physics.Raycast(origin, -MoveSide, out RaycastHit hit4, distance))
            {
                if (hit4.collider.CompareTag("white") || hit4.collider.CompareTag("red"))
                {
                    GameObject target = hit4.collider.gameObject;
                    Vector3 nyan = target.transform.position;

                    // 【注意】現状のコードではレイ検出時の処理（ifブロック内）が空のため、
                    // その側面ボールの進行方向に「何か障害物がある場合」に、進行方向へ移動させる挙動になっています
                    if (Physics.Raycast(nyan, MoveFront, out RaycastHit what, distance)) { }
                    else
                    {
                        target.transform.position += new Vector3(XMove, 0, ZMove);
                    }
                }
            }

            // 自マスの座標を移動方向へ1マス分ずらす
            origin += new Vector3(XMove, 0, ZMove);

            // 効果音再生と残り移動回数の減算
            if (movesound) this.sound.PlayOneShot(MoveSound);

            var tp = Maneger.GetComponent<StageManegar>();
            if (tp != null) tp.movedown();
        }

        // 最終決定した座標を実体に適用
        transform.position = origin;
        Debug.Log(away);

    }



    // ==========================================
    // フレーム毎の更新処理（入力・移動判定）
    // ==========================================
    void Update()
    {
        // ------------------------------------------
        // 1. ゲーム状態の確認（ポーズ・クリア・ゲームオーバー）
        // ------------------------------------------
        if (StageManegar.Instance != null)
        {
            // いずれかのメニューが開いている場合は操作不可（freeze）にする
            if (StageManegar.Instance.activePause == true || StageManegar.Instance.activeClear == true || StageManegar.Instance.activeOver)
            {
                freeze = true;
            }
            else
            {
                freeze = false;
            }
        }

        // ------------------------------------------
        // 2. 音声設定の同期
        // ------------------------------------------
        // 管理クラスから移動音を鳴らすかどうかの設定を同期
        if (StageManegar.Instance.activemovesound == true)
        {
            movesound = true;
        }
        else
        {
            movesound = false;
        }

        // 管理クラスのSE音量を現在のオーディオソースに適用
        sound.volume = StageManegar.Instance.SE.volume;


        // ------------------------------------------
        // 3. プレイヤーの入力処理
        // ------------------------------------------
        if (!freeze) // 操作可能な状態である場合
        {
            // 【同時入力ガード】すでに移動キーが押されている場合の処理
            if (move)
            {
                // すべての移動キー（WASD）が離されたら、次の入力を受け付ける（moveをfalseに戻す）
                if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) &&
                    !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
                {
                    move = false;
                }
                return; // キーが離されるまではこれ以降の移動処理をスキップ
            }


            // ------------------------------------------
            // 【Aキー】左方向への移動処理
            // ------------------------------------------
            if (Input.GetKeyDown(KeyCode.A) && !move)
            {
                StickyBall(Vector3.left);
            }

            // ------------------------------------------
            // 【Dキー】右方向への移動処理
            // ------------------------------------------
            else if (Input.GetKeyDown(KeyCode.D) && !move)
            {
                StickyBall(Vector3.right);
            }

            // ------------------------------------------
            // 【Sキー】下（後方）方向への移動処理
            // ------------------------------------------
            else if (Input.GetKeyDown(KeyCode.S) && !move)
            {
                StickyBall(Vector3.back);
            }

            // ------------------------------------------
            // 【Wキー】上（前方）方向への移動処理
            // ------------------------------------------
            else if (Input.GetKeyDown(KeyCode.W) && !move)
            {
                StickyBall(Vector3.forward);
            }

            // ------------------------------------------
            // 【Cキー】位置入れ替え処理
            // ------------------------------------------
            else if (Input.GetKeyDown(KeyCode.C))
            {
                Vector3 origin = transform.position;       // 自分の現在位置
                Vector3 point = Change.transform.position; // 入れ替え対象（Change boll）の現在位置

                // お互いの座標を入れ替える
                transform.position = point;
                Change.transform.position = origin;

                // 入れ替え時のSEを再生
                this.sound.PlayOneShot(RotateSound);
            }
        }
    }


    // ==========================================
    // デバッグ用：Sceneビューへのギズモ表示
    // ==========================================
    void OnDrawGizmos()
    {
        // レイの検知範囲（1.0f）を視覚化するために、各方向へ赤い線を描画
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.right * 1.0f);   // 右
        Gizmos.DrawRay(transform.position, Vector3.left * 1.0f);    // 左
        Gizmos.DrawRay(transform.position, Vector3.forward * 1.0f); // 前
        Gizmos.DrawRay(transform.position, Vector3.back * 1.0f);    // 後
        Gizmos.DrawRay(transform.position, Vector3.down * 1.0f);    // 下
    }
}