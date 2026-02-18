# Project: LINE LIFF × QR × SmartLock 無人運営システム

## 1. 目的
LINEアプリ内（LIFF）で購入→QR発行→店舗iPadでスキャン→サーバ判定→解錠までを実現する無人入室システムを設計・実装・デプロイする。

新規アプリのインストールは禁止。

---

## 2. 固定仕様（変更不可）

### 料金
- 1時間パス：600円（60分）
- 1日パス：2400円（決済確定から24時間）
- 月額：9800円（サブスク、自動更新Webhook処理）

### QR
- 利用回数：1回（max_uses=1）
- TTL：60〜120秒
- 期限切れはLIFF側で再発行可能
- 署名付きトークン（JWT or PASETO）
- door_id と store_id に必ず紐付ける

### 店舗構成
- 店舗iPad 1台（Webスキャナ）
- 解錠デバイスは MockSmartLockAdapter で実装（将来差し替え）

---

## 3. ユーザーロール

- Customer（LINE LIFF内利用）
- Admin（管理画面）
- Device（店舗iPad Web）

---

## 4. 技術スタック（提案→採用して実装）

Backend:
- ASP.NET Core Web API
- PostgreSQL
- JWT署名

Frontend:
- React
- LIFF SDK（LINEログイン必須）

決済:
- Stripe（Checkout + Webhook）
- PaymentProviderAdapterで抽象化

解錠:
- SmartLockAdapter（Mock実装）

---

## 5. 基本フロー

### 購入
1. LIFF起動（LINEログイン）
2. プラン選択
3. Stripe Checkoutへ遷移
4. Webhook受信で決済確定
5. AccessPass発行

### QR表示
- POST /passes/{id}/qr
- 署名付き短命トークン発行
- LIFF画面に表示
- 再発行ボタンあり

### 入室
1. iPadでQRスキャン
2. /device/scanへ送信
3. サーバで検証
   - 署名
   - exp
   - pass有効性
   - used_count=0
4. OKなら
   - used_count=1更新（トランザクション）
   - AccessLog記録
   - unlock()実行

---

## 6. Passロジック

- 60分：決済時刻 +60分
- 1日：決済時刻 +24時間
- 月額：+1ヶ月（更新Webhookで延長）
- すべて max_uses=1
- 退室管理はMVP不要

---

## 7. データモデル（最低限）

Users  
Stores  
Doors  
Plans  
Orders  
AccessPasses  
AccessLogs  
WebhookEvents  

AccessPass:
- valid_from
- valid_to
- max_uses=1
- used_count

---

## 8. API（最低限）

POST /auth/line-login  
GET /plans  
POST /checkout/create  
POST /webhooks/stripe  
GET /passes/me  
POST /passes/{id}/qr  
POST /device/scan  

Admin:
GET /admin/passes  
POST /admin/passes/{id}/revoke  
GET /admin/logs  

---

## 9. セキュリティ制約（厳守）

- 決済未確定で解錠しない
- QR漏洩前提設計（短TTL・1回・door紐付け）
- Deviceは判断しない（必ずサーバ判定）
- Webhook署名検証必須
- scan処理はトランザクション
- フェイルセーフ（解錠しないがデフォルト）

---

## 10. 必須成果物

1. MVP/Next分割表
2. 画面設計
3. DBスキーマ + migration
4. Backend実装 + OpenAPI
5. Frontend（LIFF / Admin / Device）
6. Docker Compose
7. 本番デプロイ手順
8. LINE設定手順（LIFF作成、Webhook URL等）
9. 運用ガイド

---

## 11. 設計思想

- Adapter構造で差し替え可能設計
- 監査ログ重視
- Multi-store拡張前提設計
- 本番運用を見据えた可観測性

---

## 12. 将来拡張（Next / Phase2-3）
※MVPでは実装しないが、拡張可能な設計にする

### 12.1 Multi-store / Multi-door
- すべての主要テーブルは store_id を持つ
- Doorは複数登録可能
- Adminは store単位RBAC

### 12.2 Device管理
- Devices(device_id, store_id, door_scope, status, last_seen)
- iPad登録フロー
- Device単位監査ログ

### 12.3 ゾーン / 席 / 個室
- Zones
- Seats / Rooms
- Passと紐付け可能設計

### 12.4 予約（Booking）
Bookings(user_id, store_id, resource_id, start_at, end_at, status)
予約→決済→Pass発行の統合設計

### 12.5 超過課金（Overage）
AccessSessions(pass_id, check_in_at, check_out_at, duration, overage_amount)
Stripe後払い拡張可能設計

### 12.6 月額の拡張
Memberships(user_id, store_id, current_period_start, current_period_end, stripe_subscription_id)

### 12.7 クーポン
Coupons(code, type, amount, usage_limit)

### 12.8 返金・遠隔解錠
Refunds
AdminActions拡張
遠隔解錠は理由必須ログ

### 12.9 運用・監査
ログ検索最適化
異常検知
バックアップ戦略

### 12.10 セキュリティ拡張
Admin 2FA
Device署名
鍵ローテーション
