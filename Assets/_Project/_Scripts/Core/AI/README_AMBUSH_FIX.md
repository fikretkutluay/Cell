# Ambush Mode Hareket Sorunu - Çözüm

## 🐛 Sorun
Enemy'ler Ambush moduna geçtiğinde hareket etmeyi bırakıyordu ve node'lar üzerinden hareket etmiyordu.

## 🔍 Tespit Edilen Problemler

### 1. Path Hesaplama Gecikmesi
**Sorun:** Ambush moduna geçildiğinde `lastPathUpdateTime` sıfırlanmıyordu, bu yüzden ilk path hesaplaması `pathUpdateInterval` (0.2 saniye) kadar gecikiyordu.

**Çözüm:** State değiştiğinde `lastPathUpdateTime = 0` yaparak ilk path hesaplamasının hemen yapılmasını sağladık.

### 2. Null Path Kontrolü Eksikliği
**Sorun:** PathfinderAStar null döndüğünde veya boş path döndüğünde bu kontrol edilmiyordu.

**Çözüm:** Path hesaplandıktan sonra `newPath != null && newPath.Count > 0` kontrolü eklendi.

### 3. Debug Bilgisi Eksikliği
**Sorun:** Ambush modunda ne olduğunu görmek zordu.

**Çözüm:** 
- Her state değişiminde Debug.Log eklendi
- Path bulunduğunda/bulunamadığında log eklendi
- Hareket etmediğinde warning eklendi

## ✅ Yapılan Değişiklikler

### LeukocyteAI.cs

#### 1. State Değişiminde Path Reset
```csharp
if (currentState == AIState.Patrol)
{
    if (canAmbush && Random.value > 0.5f)
    {
        currentState = AIState.Ambush;
        Debug.Log("Switched to AMBUSH mode");
    }
    currentPath.Clear();
    lastPathUpdateTime = 0; // ✨ YENİ: İlk path hesaplamasını hemen yap
}
```

#### 2. DoAmbush() İyileştirmeleri
```csharp
private void DoAmbush()
{
    if (Time.time > lastPathUpdateTime + pathUpdateInterval)
    {
        // ... path hesaplama ...
        
        if (newPath != null && newPath.Count > 0) // ✨ YENİ: Null ve boş kontrol
        {
            currentPath = newPath;
            Debug.Log($"Ambush: Path found with {newPath.Count} nodes");
        }
        else
        {
            Debug.LogWarning("Ambush: Path not found! Falling back to direct movement.");
            currentPath.Clear();
        }
    }
}
```

#### 3. MoveAlongPath() Debug Bilgileri
```csharp
private void MoveAlongPath()
{
    if (currentPath != null && currentPath.Count > 0)
    {
        // ... hareket kodu ...
        Debug.Log($"{currentState}: Reached node. Remaining nodes: {currentPath.Count}");
    }
    else
    {
        // Path yoksa direkt hareket
        if (currentState == AIState.Chase || currentState == AIState.Ambush)
        {
            // ... direkt hareket kodu ...
            Debug.LogWarning($"{currentState}: Not moving! Stuck at {transform.position}");
        }
    }
}
```

### LeukocyteAIDebugger.cs (YENİ)
Enemy'nin state ve path bilgilerini Scene view'da görselleştiren yardımcı script.

**Özellikler:**
- State bilgisini enemy'nin üstünde gösterir
- Path'i renkli çizgilerle gösterir
- Node'ları küre olarak gösterir

## 🎮 Unity'de Kullanım

### 1. Mevcut Enemy'leri Güncelle
Hiçbir şey yapmanıza gerek yok! Script otomatik güncellenecek.

### 2. Debug Modu Aktif Et (Opsiyonel)
Enemy prefab'ına veya scene'deki enemy'lere:
1. Add Component → **LeukocyteAIDebugger**
2. Inspector'da:
   - Show Debug Info: ✓ (İşaretli)
   - Draw Path: ✓ (İşaretli)
   - Path Color: Cyan (veya istediğiniz renk)

### 3. Console'u İzle
Play modunda Console'da şu log'ları göreceksiniz:
- `Switched to AMBUSH mode` - Ambush moduna geçildi
- `Ambush: Path found with X nodes` - Path bulundu
- `Ambush: Path not found!` - Path bulunamadı (sorun var!)
- `Ambush: Not moving! Stuck at (x,y)` - Hareket etmiyor (sorun var!)

## 🔧 Sorun Giderme

### Problem: Hala hareket etmiyor
**Kontrol Et:**
1. ✓ GraphData asset'i enemy'ye atanmış mı?
2. ✓ GraphData'da node'lar var mı?
3. ✓ Node'ların neighbors listesi dolu mu?
4. ✓ Player referansı atanmış mı?
5. ✓ Enemy'nin moveSpeed > 0 mı?

**Console'da şu log'ları ara:**
- `Ambush: Node is null!` → GraphData veya node'lar eksik
- `Ambush: Path not found!` → Node'lar birbirine bağlı değil
- `Wall between enemy and player` → Duvar engel oluyor

### Problem: Path bulunuyor ama hareket yok
**Kontrol Et:**
1. ✓ Enemy'de Rigidbody2D var mı?
2. ✓ Rigidbody2D kinematic değil mi?
3. ✓ Enemy'nin position'ı kilitli değil mi?
4. ✓ Time.timeScale = 1 mi?

### Problem: Enemy node'lara gitmiyor, direkt player'a gidiyor
**Sebep:** Path bulunamıyor, fallback olarak direkt hareket kullanılıyor.

**Çözüm:**
1. GraphData'yı kontrol et
2. Node'ların neighbors listesini kontrol et
3. PathfinderAStar'ın çalıştığını doğrula

## 📊 Beklenen Davranış

### Ambush Modu Akışı:
1. Player sight range'e girer
2. %50 şans ile Ambush moduna geçer
3. **HEMEN** path hesaplanır (0 gecikme)
4. Player'ın hızına göre tahmin edilen pozisyon hesaplanır
5. O pozisyona path bulunur
6. Enemy path boyunca hareket eder
7. Her 0.2 saniyede path güncellenir

### Path Bulunamazsa:
- Console'da warning görünür
- Enemy direkt player'a doğru hareket eder (duvar yoksa)
- Bir sonraki update'te tekrar path aranır

## 🎯 Test Senaryoları

### Test 1: Normal Ambush
1. Play'e bas
2. Player'ı enemy'ye yaklaştır
3. Console'da "Switched to AMBUSH mode" görünmeli
4. Enemy hareket etmeli
5. Scene view'da path çizgileri görünmeli (debugger varsa)

### Test 2: Path Bulunamama
1. Enemy'yi node'lardan uzağa koy
2. Player'ı yaklaştır
3. Console'da "Path not found!" görünmeli
4. Enemy yine de hareket etmeli (direkt)

### Test 3: Duvar Engeli
1. Enemy ile player arasına duvar koy
2. Console'da "Wall between enemy and player" görünmeli
3. Enemy path kullanarak duvarın etrafından gelmeli

## 📝 Notlar

- Ambush modu player'ın hareketini tahmin eder
- Chase modu direkt player'ın pozisyonunu hedefler
- Path bulunamazsa her iki mod da direkt harekete geçer
- Debug log'ları production'da kapatılabilir (Debug.Log → // Debug.Log)
