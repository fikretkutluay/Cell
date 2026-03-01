# Graph Save System Fix

## Sorun
GraphData node'ları ve bağlantıları sahne her açılıp kapandığında kayboluyordu veya değişiyordu.

## Neden Oluyordu?
Unity'nin ScriptableObject serileştirme sistemi, circular reference'ları (node'ların birbirine referans vermesi) düzgün kaydedemiyordu. `[SerializeReference]` kullanımı da sorunu çözmüyordu.

## Çözüm
**Index-Based System**: Node'lar artık birbirine direkt referans yerine index numaralarıyla bağlanıyor.

### Değişiklikler

**GraphNode.cs**
- `neighbors` listesi artık runtime'da kullanılıyor (`[NonSerialized]`)
- Yeni `neighborIndices` listesi eklendi - bu kaydediliyor
- Index'ler node'ların GraphData.nodes listesindeki pozisyonlarını tutuyor

**GraphData.cs**
- `RebuildNeighborReferences()` methodu eklendi
- Bu method index'lerden neighbor referanslarını yeniden oluşturuyor
- `OnEnable()` ile asset her yüklendiğinde otomatik çalışıyor

**GraphManagerEditor.cs**
- Node bağlantıları artık index kullanarak kaydediliyor
- Her değişiklikten sonra `AssetDatabase.SaveAssets()` çağrılıyor
- Debug log'ları eklendi

**GraphManager.cs**
- `Awake()` içinde `RebuildNeighborReferences()` çağrılıyor
- Böylece oyun başladığında neighbor'lar hazır oluyor

**ThickMazeGenerator.cs**
- Index sistemiyle uyumlu hale getirildi
- Node'lar oluşturulurken neighborIndices kullanılıyor
- `RebuildNeighborReferences()` çağrılıyor
- Kayıt sonrası log eklendi

## Kullanım

### Manuel Node Ekleme
1. Scene view'da GraphManager seçili olmalı
2. **Shift + Sol Tık** ile node ekle
3. Console'da "Node added" mesajını gör

### Manuel Node Bağlama
1. Bir node'a **Sol Tık** ile seç (sarı olur)
2. Başka bir node'a **Sağ Tık** ile bağla
3. Console'da "Connection created" mesajını gör

### ThickMazeGenerator ile Otomatik Oluşturma
1. Hierarchy'de ThickMazeGenerator objesini seç
2. Inspector'da tüm referansları ata (Tilemap, Tiles, GraphData)
3. Sağ tık → "Haritayı Oluştur (Tam Otomasyon)"
4. Console'da "GraphData saved with X nodes!" mesajını gör
5. Sahneyi kapat/aç - node'lar kaybolmamalı

### Kayıt Kontrolü
- Her işlemden sonra otomatik kaydediliyor
- Project penceresinde GraphData asset'ine tıkla
- Inspector'da `neighborIndices` listelerini görebilirsin
- Sahneyi kapat/aç - node'lar aynı kalmalı

## Test
1. ThickMazeGenerator ile harita oluştur
2. Console'da "GraphData saved with X nodes!" mesajını kontrol et
3. Sahneyi kaydet ve kapat
4. Sahneyi tekrar aç
5. Node'lar ve bağlantılar aynı olmalı (Gizmos ile görülebilir)

## Önemli Notlar
- Eski GraphData asset'leri çalışmayabilir - yeniden oluşturman gerekebilir
- Node silme özelliği henüz yok (gerekirse eklenebilir)
- Index sistemi sayesinde artık güvenilir kayıt var
- ThickMazeGenerator artık index sistemiyle tam uyumlu
