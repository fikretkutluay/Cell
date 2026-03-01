# 🧠 Beyin Boss Sorun Giderme Rehberi

## ❌ Sorun 1: Projectile'lar Fırlamıyor

### Olası Sebepler ve Çözümler:

#### 1. Rigidbody2D Ayarları Yanlış

**Kontrol Et:**
- Projectile prefab'ını aç
- Rigidbody2D component'ini kontrol et

**Doğru Ayarlar:**
```
Body Type: Dynamic (KINEMATIC DEĞİL!)
Gravity Scale: 0
Simulated: ✓ (İşaretli)
Collision Detection: Continuous
Constraints: Hiçbiri (hepsi kapalı)
```

**Hızlı Düzeltme:**
1. Projectile prefab'ını seç
2. Inspector'da Rigidbody2D bul
3. Body Type: **Dynamic** yap
4. Gravity Scale: **0** yap
5. Prefab'ı kaydet (Ctrl+S)

#### 2. Prefab Atanmamış

**Kontrol Et:**
- BrainBoss GameObject'ini seç
- Inspector'da BrainBoss script'i bul
- Prefab alanlarını kontrol et

**Console'da Göreceksin:**
```
Burst Projectile Prefab is null!
Targeted Projectile Prefab is null!
```

**Çözüm:**
1. Projectile prefab'larını oluştur (README'de anlatıldığı gibi)
2. BrainBoss Inspector'da prefab'ları ata

#### 3. Rigidbody2D Yok

**Console'da Göreceksin:**
```
Projectile prefab doesn't have Rigidbody2D!
```

**Çözüm:**
1. Projectile prefab'ını aç
2. Add Component → Rigidbody2D
3. Ayarları yap (yukarıda)

#### 4. Velocity Sıfırlanıyor

**Sebep:** Başka bir script velocity'yi override ediyor olabilir.

**Kontrol Et:**
- Projectile prefab'ında başka script var mı?
- O script Rigidbody2D'yi değiştiriyor mu?

**Çözüm:**
- Gereksiz scriptleri kaldır
- Veya scriptlerde velocity ayarını kaldır

---

## ❌ Sorun 2: Spawn Olan Enemy'ler Player'a Gitmiyor

### Olası Sebepler ve Çözümler:

#### 1. Enemy AI'da Player Referansı Yok

**Sebep:** Spawn edilen enemy'lere player referansı verilmiyor.

**Çözüm:** Script güncellendi! Artık otomatik atanıyor.

**Kontrol Et:**
```csharp
// BrainBoss.cs içinde Attack1_SpawnLeukocytes fonksiyonunda:
LeukocyteAI enemyAI = leukocyte.GetComponent<LeukocyteAI>();
if (enemyAI != null && player != null)
{
    enemyAI.player = player;
}
```

**Console'da Göreceksin:**
```
Enemy AI player reference set! ✓
```

Veya hata varsa:
```
LeukocyteAI component not found on spawned enemy! ❌
```

#### 2. Enemy Prefab'ında LeukocyteAI Yok

**Kontrol Et:**
- Enemy.prefab dosyasını aç
- LeukocyteAI scripti var mı?

**Çözüm:**
1. Enemy prefab'ını aç
2. Add Component → Leukocyte AI
3. GraphData'yı ata
4. Ayarları yap

#### 3. GraphData Atanmamış

**Sebep:** Enemy AI hareket etmek için GraphData'ya ihtiyaç duyar.

**Kontrol Et:**
- Enemy prefab'ını aç
- LeukocyteAI script'inde GraphData atanmış mı?

**Çözüm:**
1. GraphData ScriptableObject oluştur
2. Node'ları doldur
3. Enemy prefab'ında LeukocyteAI'a ata

#### 4. Player Tag'i Yanlış

**Kontrol Et:**
- Player GameObject'ini seç
- Inspector üstte Tag: **Player** mi?

**Çözüm:**
1. Player GameObject'ini seç
2. Tag dropdown → Player
3. Yoksa Add Tag → "Player" ekle

---

## 🧪 Test Adımları

### Test 1: Projectile Fırlatma

1. **Play** butonuna bas
2. **Console'u aç** (Ctrl+Shift+C)
3. Boss saldırı yaptığında şunları göreceksin:

**Başarılı:**
```
Brain Boss: 360 Degree Burst!
Projectile 0 fired with velocity: (8, 0)
Projectile 1 fired with velocity: (7.39, 3.06)
...
```

**Başarısız:**
```
Burst Projectile Prefab is null! ❌
Projectile prefab doesn't have Rigidbody2D! ❌
```

### Test 2: Enemy Spawn

1. **Play** butonuna bas
2. Boss spawn saldırısı yaptığında:

**Başarılı:**
```
Brain Boss: Spawning Leukocytes!
Enemy AI player reference set!
Enemy AI player reference set!
Enemy AI player reference set!
```

**Başarısız:**
```
LeukocyteAI component not found on spawned enemy! ❌
```

3. **Scene view'da kontrol et:**
   - Enemy'ler spawn oldu mu?
   - Player'a doğru hareket ediyorlar mı?

---

## 🔧 Hızlı Düzeltme Checklist

### Projectile Sorunları İçin:

- [ ] Projectile prefab'ı var mı?
- [ ] Rigidbody2D ekli mi?
- [ ] Body Type: **Dynamic** mi?
- [ ] Gravity Scale: **0** mı?
- [ ] Collider2D ekli mi?
- [ ] Collider Is Trigger: **true** mu?
- [ ] BrainProjectile scripti ekli mi?
- [ ] BrainBoss'ta prefab atanmış mı?

### Enemy Spawn Sorunları İçin:

- [ ] Enemy prefab'ı var mı?
- [ ] LeukocyteAI scripti ekli mi?
- [ ] GraphData atanmış mı?
- [ ] Player tag'i "Player" mi?
- [ ] BrainBoss'ta enemy prefab atanmış mı?
- [ ] Console'da "Enemy AI player reference set!" görünüyor mu?

---

## 🎯 Manuel Test

### Projectile Test:

1. Projectile prefab'ını Hierarchy'e sürükle
2. Inspector'da Rigidbody2D'yi bul
3. Play moduna geç
4. Rigidbody2D'de Velocity'yi manuel ayarla: (5, 0)
5. Projectile hareket etmeli

**Hareket etmiyorsa:**
- Body Type: Dynamic yap
- Simulated: İşaretle
- Gravity Scale: 0 yap

### Enemy Test:

1. Enemy prefab'ını Hierarchy'e sürükle
2. Inspector'da LeukocyteAI'ı bul
3. Player referansını manuel ata
4. Play moduna geç
5. Enemy player'a doğru hareket etmeli

**Hareket etmiyorsa:**
- GraphData atanmış mı kontrol et
- Player referansı doğru mu kontrol et
- Console'da hata var mı kontrol et

---

## 📊 Debug Log'ları

Script'te debug log'ları eklendi. Console'da şunları göreceksin:

### Başarılı Durumlar:
```
✓ Brain Boss: 360 Degree Burst!
✓ Projectile 0 fired with velocity: (8, 0)
✓ Brain Boss: Spawning Leukocytes!
✓ Enemy AI player reference set!
✓ Brain Boss: Targeted Shot!
✓ Targeted projectile fired with velocity: (15, 0)
```

### Hata Durumları:
```
❌ Burst Projectile Prefab is null!
❌ Projectile prefab doesn't have Rigidbody2D!
❌ LeukocyteAI component not found on spawned enemy!
❌ Player is null!
❌ Targeted Projectile Prefab is null!
```

---

## 💡 Pro İpuçları

### İpucu 1: Prefab Override
Eğer prefab'ı değiştirdiysen ama değişiklikler uygulanmadıysa:
1. Prefab'ı aç
2. Değişiklikleri yap
3. **Ctrl+S** ile kaydet
4. Scene'i yeniden yükle

### İpucu 2: Rigidbody2D Simulated
Bazen Rigidbody2D "Simulated" kapalı olabilir:
1. Projectile prefab'ını aç
2. Rigidbody2D → Simulated: **İşaretle**

### İpucu 3: Collision Matrix
Projectile'lar boss'a çarpıyorsa:
1. Edit → Project Settings → Physics 2D
2. Collision Matrix'te Boss ve Projectile layer'larını kontrol et

### İpucu 4: Scene View'da İzle
Play modunda Scene view'ı aç:
- Projectile'lar spawn oluyor mu?
- Velocity'leri var mı?
- Yönleri doğru mu?

---

## 🆘 Hala Çalışmıyorsa

1. **Console'u temizle** (Clear)
2. **Play** butonuna bas
3. **Console'daki ilk hatayı** oku
4. **Yukarıdaki çözümleri** uygula
5. **Tekrar test et**

Hala sorun varsa:
- Prefab'ları sıfırdan oluştur
- Script'leri tekrar ekle
- README'deki adımları takip et

---

## ✅ Çalışıyor Kontrolü

Şunları görüyorsan her şey çalışıyor demektir:

- ✅ Boss titriyor
- ✅ Projectile'lar fırlıyor
- ✅ Enemy'ler spawn oluyor
- ✅ Enemy'ler player'a gidiyor
- ✅ Projectile'lar player'a hasar veriyor
- ✅ Console'da hata yok

Tebrikler! Beyin Boss çalışıyor! 🎉
