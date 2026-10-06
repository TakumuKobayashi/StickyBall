using UnityEngine;

//AIにコメントを書いてもらいました。

public class redball : MonoBehaviour
{
    // インスペクターから設定するダミーのボール（演出用など）のプレハブまたはオブジェクト
    [SerializeField] private GameObject DummyBall;

    // 他のコライダー（トリガー）に接触したときに呼ばれるイベント
    private void OnTriggerEnter(Collider collision)
    {
        // 接触したオブジェクトのタグが "goal" の場合
        if (collision.CompareTag("goal"))
        {

            // ステージ管理者（StageManegar）のクリア条件状況を確認
            // clearの値が2、かつ clearredの値が0の特定のタイミングの時
            if (StageManegar.Instance.clear == 2 && StageManegar.Instance.clearred == 0)
            {
                // DummyBallがインスペクターで正しく割り当てられているかチェック
                if (DummyBall != null)
                {
                    // ダミーのボールを、現在のこのオブジェクト（赤玉）と同じ位置に移動
                    DummyBall.transform.position = this.transform.position;

                    // このオブジェクト（赤玉）の位置を、現在の位置からY軸方向に「-5」移動させて画面外や地下に引っ込める
                    this.transform.position += new Vector3(0, -5, 0);
                }
            }

            // ステージ管理者に赤玉がゴールに入った（または条件を満たした）ことを通知
            StageManegar.Instance.redin();

        }
    }
}