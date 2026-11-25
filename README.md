# MornNovel

## 概要

ノベルゲーム/ビジュアルノベルエンジン用のシステムライブラリ。ノベル再生、キャラクター表示、メッセージ表示、背景切り替え、BGM・SE管理を統合的に提供。

## 依存関係

| 種別 | 名前 |
|------|------|
| 外部パッケージ | Arbor, UniTask, VContainer, UniRx, TextMesh Pro, Cinemachine（オプション）, Addressables（オプション） |
| Mornライブラリ | MornArbor, MornBeat, MornSound, MornDebug, MornTransition, MornLocalize, MornScene, MornColor, MornGlobal, MornUtil, MornTween, MornUGUI |

## 使い方

### セットアップ

1. `MornNovelGlobal` でノベルシーン、アドレス設定を定義
2. `MornNovelSettings` でBGMフェード時間、キャラ移動設定などを調整
3. `MornNovelLifetimeScope` でVContainer DI設定

### ノベル再生

```csharp
MornNovelService.SetNovelAddress();
// → ノベルシーン読み込み
// → Arborコマンドで順序制御
```

### 主要なArborコマンド

| コマンド | 機能 |
|---------|------|
| MornNovelStartCommand | ノベル開始 |
| MornNovelMessageCommand | セリフ表示 |
| MornNovelCharaShowCommand | キャラ登場 |
| MornNovelSetBackgroundCommand | 背景設定 |
| MornNovelBgmCommand | BGM再生/停止 |
| MornNovelCameraShakeCommand | Cinemachine カメラ揺れ |
| MornNovelShakeCommand | 画面・キャラ揺れ |
| MornNovelEndCommand | ノベル終了 |

- セリフの Talker が話者。非話者の明るさは `MornNovelSettings.UnfocusedBrightness`（初期値 0.65）。複数人 Talker では全員を通常の明るさに戻す。
- 揺らしたい箇所に `MornNovelShakeCommand` を置き、Next State をつなぐ。対象なしで背景と立ち絵、対象に Talker を指定するとその立ち絵だけを揺らす。吹き出しは固定。初期値は強さ 0.18（Unity 単位）、長さ 0.25 秒。揺れが終わってから次のステートへ進む。

### 既読管理

```csharp
// 既読判定
bool isRead = MornNovelService.IsNovelRead();

// 既読フラグ登録
MornNovelService.AtNovelReadEnd();
```
