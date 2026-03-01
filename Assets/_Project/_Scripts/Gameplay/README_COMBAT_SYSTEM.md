# Combat System - Kurulum Rehberi

## 📋 Yapılan Değişiklikler

### 1. **EnemyCollisionDamage.cs** (GÜNCELLENDİ) 🔄
Enemy'nin player'a çarpınca hasar vermesini sağlar. **YENİ:** Enemy de çarpışmadan hasar alır ve canı bitince ölür.

**Özellikler:**
- Player'a çarpınca otomatik hasar verir
- **Enemy kendine de hasar alır** (kamikaze tarzı)
- Sürekli temas halindeyken belirli aralıklarla hasar verir (spam önleme)
- Enemy'nin canı bitince EnemyHealth scripti üzerinden ölür
- Hasar miktarları Inspector'dan düzenlenebilir

### 2. **EnemyHealth.cs** (GÜNCELLENDİ)
Enemy'nin can sistemini yönetir.

**İyileştirmeler:**
- Tekrar ölme bug'ı düzeltildi (isDying flag)
- Ölürken collider ve rigidbody devre dışı bırakılıyor
- Debug log'ları eklendi
- Daha temiz kod yapısı

### 3. **PlayerController.cs** (ZATEN HAZIR)
Player zaten hasar alıyor ve ölüyor. Değişiklik gerekmedi.

---

## 🎮 Unity Inspector'da Kurulum

### Enemy Prefab'ına Component Ekle

1. **Enemy.prefab** dosyasını aç (Assets/_Project/_Prefabs/Enemy.prefab)

2. Inspector'da **Add Component** butonuna tıkla

3. **EnemyCollisionDamage** scriptini ekle

4. Inspector'da ayarları yap:
   ```
   Damage To Player: 10 (Player'a verilecek hasar)
   Damage To Self: 15 (Enemy'nin kendine vereceği hasar)
   Damage Interval: 1 (Hasar verme aralığı - saniye)
   ```

5. **EnemyHealth** scriptini kontrol et:
   - Max Health: 30 (veya istediğin değer)
   - Enemy 2 kez çarpışınca ölür (15 + 15 = 30)

6. **Collider2D** kontrolü:
   - Enemy'de CircleCollider2D veya BoxCollider2D olmalı
   - **Is Trigger: KAPALI** olmalı (collision için)

7. **Rigidbody2D** kontrolü:
   - Body Type: **Dynamic** veya **Kinematic**
   - Gravity Scale: **0** (top-down oyun)

8. **Tag** kontrolü:
   - Enemy GameObject'inin Tag'i: **"Enemy"** olmalı
   - Player GameObject'inin Tag'i: **"Player"** olmalı

---

## ✅ Test Etme

1. Play butonuna bas
2. Enemy'yi player'a doğru hareket ettir (manuel veya AI ile)
3. Çarpışma olduğunda:
   - ✓ Player hasar almalı (Console'da log görünür)
   - ✓ Enemy kendine hasar almalı (Console'da log görünür)
   - ✓ Player'ın canı azalmalı (UI'da görünür)
   - ✓ Enemy'nin canı azalmalı
   - ✓ 2. çarpışmada enemy ölmeli (varsayılan değerlerle)

4. Sürekli temas halinde:
   - ✓ Her 1 saniyede bir hasar verilmeli
   - ✓ Spam hasar yok (damage interval sayesinde)

---

## 🔧 Ayarlanabilir Parametreler

### EnemyCollisionDamage
- **Damage To Player**: Enemy'nin player'a vereceği hasar (varsayılan: 10)
- **Damage To Self**: Enemy'nin kendine vereceği hasar (varsayılan: 15)
- **Damage Interval**: Hasar verme aralığı saniye cinsinden (varsayılan: 1)

### EnemyHealth
- **Max Health**: Enemy'nin maksimum canı (varsayılan: 30)
- **Loot Table**: Ölünce düşüreceği item tablosu
- **Destroy Delay**: Ölünce yok olmadan önce bekleme (varsayılan: 0.1)

### Örnek Ayarlar:

**Kolay Mod (Enemy çabuk ölür):**
```
Damage To Self: 30
Max Health: 30
→ 1 çarpışmada ölür
```

**Normal Mod (Dengeli):**
```
Damage To Self: 15
Max Health: 30
→ 2 çarpışmada ölür
```

**Zor Mod (Enemy dayanıklı):**
```
Damage To Self: 10
Max Health: 50
→ 5 çarpışmada ölür
```

---

## 🐛 Sorun Giderme

**Problem:** Enemy player'a hasar vermiyor
- ✓ Player'ın Tag'i "Player" mi?
- ✓ Enemy'de EnemyCollisionDamage scripti var mı?
- ✓ Collider'lar Is Trigger kapalı mı?
- ✓ PlayerController'da IDamageable interface implement edilmiş mi?

**Problem:** Enemy kendine hasar almıyor
- ✓ Enemy'de EnemyHealth scripti var mı?
- ✓ Console'da "EnemyHealth component not found!" hatası var mı?
- ✓ Max Health > 0 mı?

**Problem:** Enemy çarpınca hemen ölüyor
- ✓ Damage To Self değerini azalt
- ✓ Max Health değerini artır
- ✓ Örnek: Damage To Self: 10, Max Health: 50

**Problem:** Çarpışma algılanmıyor
- ✓ Her iki obje de Collider2D'ye sahip mi?
- ✓ En az birinde Rigidbody2D var mı?
- ✓ Collision Matrix ayarları doğru mu? (Edit → Project Settings → Physics 2D)

**Problem:** Sürekli hasar spam oluyor
- ✓ Damage Interval değerini artır (örn: 1.5 veya 2)
- ✓ OnCollisionStay2D çalışıyor mu kontrol et

---

## 📝 Notlar

- Enemy'nin hareket AI'ı ayrı bir scriptte olmalı (örn: LeukocyteAI)
- Bu script sadece çarpışma hasarını yönetir
- Player'ın hasar alma sistemi PlayerStats ScriptableObject üzerinden çalışıyor
- Enemy'nin hasar alma sistemi EnemyHealth scripti üzerinden çalışıyor
- Enemy ölünce EnemyHealth scripti loot drop yapabilir
- Kamikaze tarzı enemy'ler için Damage To Self'i artırabilirsin

---

## 🎯 Davranış Özeti

### Çarpışma Anında:
1. Enemy player'a çarpar
2. Player hasar alır (Damage To Player)
3. Enemy kendine hasar alır (Damage To Self)
4. Her iki tarafın canı azalır

### Sürekli Temas:
1. Enemy player ile temas halinde
2. Her X saniyede bir hasar verilir (Damage Interval)
3. Enemy her seferinde kendine de hasar alır
4. Enemy'nin canı bitince ölür (EnemyHealth.Die())

### Enemy Ölümü:
1. Enemy'nin canı 0'a düşer
2. EnemyHealth.Die() çağrılır
3. Loot drop olur (varsa)
4. Collider ve Rigidbody devre dışı kalır
5. GameObject yok edilir
