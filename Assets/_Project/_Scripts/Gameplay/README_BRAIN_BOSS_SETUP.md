# 🧠 Beyin Boss Kurulum Rehberi

## 📋 Genel Bakış

Beyin Boss oyunun final boss'u olup 3 farklı saldırı patternine sahiptir:
1. **Spawn Leukocytes** - Etrafına 3 akyuvar spawn eder
2. **360 Degree Burst** - Etrafına 360 derece top atar
3. **Targeted Shot** - Player'a hızlı ve güçlü top atar

Boss sürekli hızlı bir şekilde titrer (shake effect).

---

## 🎮 ADIM 1: Boss GameObject Oluştur

### 1.1 Hierarchy'de Boss Oluştur

1. Hierarchy'de sağ tıkla → **Create Empty**
2. İsim: **BrainBoss**
3. Position: Sahnenin ortasına yerleştir (örn: 0, 0, 0)

### 1.2 Sprite Ekle

**Seçenek A: Sprite Renderer (Basit)**
1. BrainBoss'a **Add Component** → **Sprite Renderer**
2. Sprite: Beyin boss sprite'ını ata
3. Sorting Layer: **Boss** (yoksa oluştur)
4. Order in Layer: **10**

**Seçenek B: Child Sprite (Animasyon için önerilen)**
1. BrainBoss'un altına sağ tıkla → **2D Object** → **Sprite**
2. İsim: **BrainSprite**
3. Sprite Renderer'a beyin sprite'ını ata
4. Sorting Layer: **Boss**

---

## 🎮 ADIM 2: Collider ve Rigidbody Ekle

### 2.1 Rigidbody2D

1. BrainBoss'a **Add Component** → **Rigidbody2D**
2. Inspector'da ayarla:
   ```
   Body Type: Kinematic (boss hareket etmez)
   Gravity Scale: 0
   Constraints → Freeze Rotation: Z ✓
   ```

### 2.2 Collider2D

1. BrainBoss'a **Add Component** → **Circle Collider 2D** (veya Box Collider 2D)
2. Inspector'da ayarla:
   ```
   Radius: 1.5 (sprite boyutuna göre ayarla)
   Is Trigger: false
   ```

### 2.3 Tag Ayarla

1. Inspector üstte **Tag** dropdown → **Add Tag**
2. Yeni tag ekle: **Boss**
3. BrainBoss GameObject'ini seç
4. Tag: **Boss**

---

## 🎮 ADIM 3: Fire Point Oluştur

Fire Point, projectile'ların spawn olacağı nokta.

1. BrainBoss'un altına sağ tıkla → **Create Empty**
2. İsim: **FirePoint**
3. Position: Boss'un önüne yerleştir (örn: 0, 1, 0)
4. Gizmos'ta görmek için:
   - Add Component → **Gizmos** (opsiyonel)
   - Veya Scene view'da seçili tutarak görebilirsin

---

## 🎮 ADIM 4: BrainBoss Script Ekle

### 4.1 Script Ekle

1. BrainBoss GameObject'ini seç
2. Inspector'da **Add Component** → **Brain Boss**

### 4.2 Inspector Ayarları

#### Stats
```
Max Health: 1000
```

#### References
```
Player: (Boş bırak - otomatik bulur)
Fire Point: FirePoint GameObject'ini sürükle
Animator: (Varsa animator'ı sürükle)
Level Exit Door: (Varsa kapı GameObject'ini sürükle)
```

#### Attack 1: Spawn Leukocytes
```
Leukocyte Prefab: Enemy.prefab'ı sürükle
Spawn Count: 3
Spawn Radius: 2
```

#### Attack 2: 360 Degree Burst
```
Burst Projectile Prefab: (Aşağıda oluşturacağız)
Burst Projectile Count: 16
Burst Projectile Speed: 8
```

#### Attack 3: Targeted Shot
```
Targeted Projectile Prefab: (Aşağıda oluşturacağız)
Targeted Projectile Speed: 15
Targeted Projectile Damage: 25
```

#### AI Settings
```
Action Cooldown: 2.5
Idle Time: 1
```

#### Shake Effect
```
Shake Amount: 0.1
Shake Speed: 20
```

---

## 🎮 ADIM 5: Projectile Prefab'ları Oluştur

### 5.1 Normal Projectile (360 Burst için)

1. Hierarchy'de sağ tıkla → **Create Empty**
2. İsim: **BrainProjectile_Normal**

3. **Sprite Renderer Ekle:**
   - Add Component → Sprite Renderer
   - Sprite: Normal top sprite'ını ata
   - Sorting Layer: **Projectiles** (yoksa oluştur)
   - Order in Layer: **5**

4. **Collider Ekle:**
   - Add Component → Circle Collider 2D
   - Radius: 0.3 (sprite boyutuna göre)
   - Is Trigger: **true** ✓

5. **Rigidbody2D Ekle:**
   - Add Component → Rigidbody2D
   - Body Type: **Dynamic**
   - Gravity Scale: **0**
   - Constraints → Freeze Rotation: **Z** ✓

6. **BrainProjectile Script Ekle:**
   - Add Component → Brain Projectile
   - Damage: **15**
   - Lifetime: **5**

7. **Tag Ayarla:**
   - Tag: **EnemyProjectile** (yoksa oluştur)

8. **Prefab Yap:**
   - Project'te `Assets/_Project/_Prefabs/` klasörüne sürükle
   - İsim: **BrainProjectile_Normal**
   - Hierarchy'den sil

### 5.2 Targeted Projectile (Güçlü saldırı için)

1. Hierarchy'de sağ tıkla → **Create Empty**
2. İsim: **BrainProjectile_Targeted**

3. **Sprite Renderer Ekle:**
   - Sprite: Güçlü top sprite'ını ata (farklı renk/boyut)
   - Sorting Layer: **Projectiles**
   - Order in Layer: **6**
   - Scale: (1.5, 1.5, 1) - Daha büyük görünsün

4. **Collider Ekle:**
   - Circle Collider 2D
   - Radius: 0.4 (daha büyük)
   - Is Trigger: **true** ✓

5. **Rigidbody2D Ekle:**
   - Body Type: **Dynamic**
   - Gravity Scale: **0**
   - Constraints → Freeze Rotation: **Z** ✓

6. **BrainProjectile Script Ekle:**
   - Damage: **25** (daha güçlü)
   - Lifetime: **5**

7. **Tag Ayarla:**
   - Tag: **EnemyProjectile**

8. **Prefab Yap:**
   - `Assets/_Project/_Prefabs/` klasörüne sürükle
   - İsim: **BrainProjectile_Targeted**
   - Hierarchy'den sil

---

## 🎮 ADIM 6: Prefab'ları Boss'a Ata

1. BrainBoss GameObject'ini seç
2. Inspector'da BrainBoss script'i bul
3. Prefab'ları ata:
   ```
   Burst Projectile Prefab: BrainProjectile_Normal
   Targeted Projectile Prefab: BrainProjectile_Targeted
   ```

---

## 🎮 ADIM 7: Animator Ekle (Opsiyonel)

### 7.1 Animator Controller Oluştur

1. Project'te sağ tıkla → Create → **Animator Controller**
2. İsim: **BrainBoss_Animator**

### 7.2 Animator Ekle

1. BrainBoss GameObject'ini seç
2. Add Component → **Animator**
3. Controller: **BrainBoss_Animator**

### 7.3 Animasyon State'leri

Animator window'da (Window → Animation → Animator):

**State'ler:**
- **Idle** (default)
- **Attack1** (Spawn)
- **Attack2** (Burst)
- **Attack3** (Targeted)
- **Die**

**Trigger'lar:**
- Attack1
- Attack2
- Attack3
- Die

**Transition'lar:**
- Idle → Attack1 (Trigger: Attack1)
- Idle → Attack2 (Trigger: Attack2)
- Idle → Attack3 (Trigger: Attack3)
- Any State → Die (Trigger: Die)
- Attack1/2/3 → Idle (Has Exit Time: true)

---

## 🎮 ADIM 8: Level Exit Door (Opsiyonel)

Boss öldüğünde aktif olacak kapı:

1. Hierarchy'de kapı GameObject'i oluştur
2. Inspector'da başlangıçta **deaktif** yap (checkbox kaldır)
3. BrainBoss Inspector'da:
   ```
   Level Exit Door: Kapı GameObject'ini sürükle
   ```

---

## ✅ Test Etme

### Test 1: Shake Effect
1. Play'e bas
2. Boss sürekli titremeye başlamalı
3. Titreme hızlı ve küçük olmalı

### Test 2: Saldırı 1 (Spawn)
1. Boss'a yaklaş
2. Boss 3 akyuvar spawn etmeli
3. Akyuvarlar boss'un etrafında daire şeklinde oluşmalı

### Test 3: Saldırı 2 (360 Burst)
1. Boss 360 derece etrafına top atmalı
2. 16 top eşit açılarla dağılmalı
3. Toplar dışa doğru hareket etmeli

### Test 4: Saldırı 3 (Targeted)
1. Boss player'a doğru hızlı top atmalı
2. Top player'ın pozisyonunu hedeflemeli
3. Daha büyük ve güçlü görünmeli

### Test 5: Hasar ve Ölüm
1. Boss'a hasar ver
2. Console'da hasar log'ları görünmeli
3. Can bitince boss ölmeli
4. Exit door aktif olmalı (varsa)

---

## 🔧 Ayarlanabilir Parametreler

### Zorluk Ayarları

**Kolay Mod:**
```
Max Health: 500
Action Cooldown: 3.5
Spawn Count: 2
Burst Projectile Count: 12
Targeted Projectile Speed: 12
```

**Normal Mod (Varsayılan):**
```
Max Health: 1000
Action Cooldown: 2.5
Spawn Count: 3
Burst Projectile Count: 16
Targeted Projectile Speed: 15
```

**Zor Mod:**
```
Max Health: 1500
Action Cooldown: 1.5
Spawn Count: 4
Burst Projectile Count: 20
Targeted Projectile Speed: 18
```

### Shake Effect Ayarları

**Hafif Titreme:**
```
Shake Amount: 0.05
Shake Speed: 15
```

**Normal Titreme (Varsayılan):**
```
Shake Amount: 0.1
Shake Speed: 20
```

**Yoğun Titreme:**
```
Shake Amount: 0.2
Shake Speed: 30
```

---

## 🐛 Sorun Giderme

### Problem: Boss titremiyor
- ✓ BrainBoss scripti ekli mi?
- ✓ Shake Amount > 0 mı?
- ✓ Play modunda mı?

### Problem: Projectile'lar spawn olmuyor
- ✓ Prefab'lar atanmış mı?
- ✓ Fire Point atanmış mı?
- ✓ Prefab'larda Rigidbody2D var mı?

### Problem: Projectile'lar boss'a çarpıyor
- ✓ Script'te Physics2D.IgnoreCollision çağrılıyor mu?
- ✓ Boss ve projectile'ların collider'ları var mı?

### Problem: Akyuvarlar spawn olmuyor
- ✓ Leukocyte Prefab atanmış mı?
- ✓ Enemy prefab'ında gerekli scriptler var mı?

### Problem: Boss saldırmıyor
- ✓ Player tag'i "Player" mi?
- ✓ Action Cooldown çok yüksek değil mi?
- ✓ Console'da hata var mı?

### Problem: Projectile'lar player'a hasar vermiyor
- ✓ Player'da IDamageable interface var mı?
- ✓ Projectile collider'ı trigger mi?
- ✓ Player tag'i "Player" mi?
- ✓ BrainProjectile scripti ekli mi?

---

## 📝 Önemli Notlar

1. **Collision Layers:**
   - Boss ve projectile'lar aynı layer'da olmamalı
   - Edit → Project Settings → Physics 2D → Collision Matrix

2. **Performance:**
   - Çok fazla projectile spawn olursa FPS düşebilir
   - Burst Projectile Count'u 20'den fazla yapma

3. **Animasyon:**
   - Animator opsiyoneldir
   - Animasyon yoksa script yine çalışır

4. **Spawn Radius:**
   - Spawn Radius boss'un collider'ından büyük olmalı
   - Yoksa akyuvarlar boss'un içinde spawn olur

5. **Fire Point:**
   - Fire Point boss'un merkezinde olabilir
   - Veya boss'un önünde olabilir (daha iyi görünüm)

---

## 🎯 Saldırı Pattern Özeti

### Pattern 1: Spawn Leukocytes
- **Ne zaman:** Rastgele
- **Ne yapar:** 3 akyuvar spawn eder
- **Tehlike:** Akyuvarlar player'ı takip eder
- **Karşı strateji:** Spawn'dan hemen sonra kaç

### Pattern 2: 360 Degree Burst
- **Ne zaman:** Rastgele
- **Ne yapar:** 16 top 360 derece atar
- **Tehlike:** Kaçacak yer az
- **Karşı strateji:** Toplar arasındaki boşluklardan geç

### Pattern 3: Targeted Shot
- **Ne zaman:** Rastgele
- **Ne yapar:** Player'a hızlı top atar
- **Tehlike:** Yüksek hasar, hızlı
- **Karşı strateji:** Yan tarafa dash at

---

## 🎨 Sprite Önerileri

### Boss Sprite:
- Boyut: 64x64 veya 128x128
- Renk: Mor, mavi-gri tonları
- Efekt: Elektrik parıltısı, nöron ağı

### Normal Projectile:
- Boyut: 16x16 veya 32x32
- Renk: Mavi, mor
- Şekil: Yuvarlak enerji topu

### Targeted Projectile:
- Boyut: 24x24 veya 48x48
- Renk: Kırmızı-mor, parlak
- Şekil: Büyük enerji topu, trail efekti

---

Beyin Boss'un kurulumu tamamlandı! 🧠✨

Sorular:
- Animasyon eklemek ister misin?
- Particle effect eklemek ister misin?
- Ses efektleri eklemek ister misin?
