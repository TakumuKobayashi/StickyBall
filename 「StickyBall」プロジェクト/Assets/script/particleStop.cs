using UnityEngine;

//AIにコメントを書いてもらいました。

public class particleStop : MonoBehaviour
{
    private ParticleSystem ps;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 自身のオブジェクトからParticleSystemコンポーネントを取得
        ps = GetComponent<ParticleSystem>();

        // パーティクルの再生を開始
        ps.Play();
    }

    // Update is called once per frame
    void Update()
    {
        // ステージ管理者（StageManegar）の状態を確認
        // ポーズ中、クリア、またはゲームオーバーのいずれかのフラグがtrueの場合
        if (StageManegar.Instance.activePause == true || StageManegar.Instance.activeClear == true || StageManegar.Instance.activeGameOver == true)
        {
            // 止まっていないときだけ Pause する
            if (!ps.isPaused)
            {
                ps.Pause(); // パーティクルの再生を一時停止
            }
        }
        else
        {
            // 上記の停止条件（ポーズ・クリア・ゲームオーバー）がいずれも該当しない場合
            // 再生中でないときだけ Play する
            if (!ps.isPlaying)
            {
                ps.Play(); // パーティクルの再生を再開（または開始）
            }
        }
    }
}