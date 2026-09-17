# Adesso World League - Kura API

[![CI](https://github.com/aykutssert/adesso-world-league-api/actions/workflows/ci.yml/badge.svg)](https://github.com/aykutssert/adesso-world-league-api/actions/workflows/ci.yml)

32 takım (8 ülke × 4 takım) 4 veya 8 gruba dağıtılır. Bir grupta aynı ülkeden iki takım olamaz. Kura
sırayla ilerler: A'ya bir takım, B'ye bir takım, son gruptan sonra tekrar A'ya. Sonuç, kurayı çeken
kişinin adıyla birlikte PostgreSQL'e yazılır.

.NET 10 · Clean Architecture · CQRS (MediatR) · EF Core + PostgreSQL · 116 test

## Çalıştırma

Tek gereksinim Docker.

```bash
docker compose up --build                      # API :8080, PostgreSQL :5433
docker compose --profile test run --rm tests   # testler
docker compose down -v                         # temizlik
```

Swagger: <http://localhost:8080/swagger> · Hazır istekler: `api.http` · Port doluysa:
`API_PORT=9090 DB_PORT=15432 docker compose up --build`

## API

| Endpoint | Ne yapar |
|---|---|
| `POST /api/draws` | Kurayı çeker, kaydeder, sonucu döner |
| `GET /api/draws/{drawId}` | Kaydedilmiş kurayı gruplarıyla döner |
| `GET /api/draws?pageNumber=1&pageSize=20` | Kura geçmişi, en yeniden eskiye |
| `GET /health` · `/health/live` | Veritabanı dahil sağlık kontrolü · sadece uygulama |

İstek `groupCount` (4 veya 8), `firstName`, `lastName` bekler, üçü de zorunlu. Yanıt `201`:

```jsonc
{
  "drawId": "01a0b0b2-4811-7aa7-8cef-44d73f294906",
  "drawnBy": { "firstName": "Aykut", "lastName": "Sert" },
  "groupCount": 8,
  "drawnAtUtc": "2026-09-17T18:47:49.009683+00:00",
  "groups": [
    { "groupName": "A", "teams": [
        { "name": "Adesso İstanbul", "country": "Türkiye" },
        { "name": "Adesso Berlin", "country": "Almanya" }
        // ...
    ] }
  ]
}
```

`groups` yapısı şartnamedeki biçimle aynı; takıma ek olarak `country` koydum ki ülke kuralı yanıttan
doğrulanabilsin. Hatalar RFC 9457 problem details: doğrulama hataları alan bazında, domain hataları
sabit bir `code` ile (`draw.not_found`) döner.

## Kura algoritması

Motor turları dolaşır, her turda grupları sırayla gezer, her grup için uygun ülkelerden birini rastgele
seçer. Uygun ülke = takımı kalan ve o grupta henüz bulunmayan ülke.

Uygun adaylar arasından rastgele seçmek tek başına yetmiyor: 8 gruplu kurada yaklaşık dörtte bir
ihtimalle son gruplarda elde sadece o grupta zaten bulunan ülkelerin takımları kalıyor ve kura tıkanıyor.
Bu yüzden motor bir adayı kabul etmeden önce, o takım havuzdan çıktığında kuranın geri kalanının hâlâ
tamamlanabilir olduğunu doğruluyor. Doğrulama bir maksimum akış problemi: kaynak → ülkeler (kapasite =
kalan takım), ülke → grup (kapasite 1, grup o ülkeyi alabiliyorsa), gruplar → çıkış (kapasite = boş
slot). Akış tüm takımları taşıyabiliyorsa kura tamamlanabilir demektir. Ağ 18 düğüm, kontrol
mikrosaniyeler sürüyor.

Böylece tıkanma imkânsız hale geliyor: bir durum çözülebiliyorsa, o çözümün sıradaki gruba atadığı ülke
mutlaka adaylar arasındadır, yani en az bir aday kontrolü geçer; backtracking gerekmez. 50.000
çekilişlik fuzz testinde tek tıkanma yok. Rastgelelik `RandomNumberGenerator`'dan gelir - kura tahmin
edilebilir olmamalı.

Kod: [`RoundRobinDrawEngine`](src/AdessoWorldLeague.Domain/Draws/Engine/RoundRobinDrawEngine.cs),
[`DrawFeasibility`](src/AdessoWorldLeague.Domain/Draws/Engine/DrawFeasibility.cs)

## Şartname maddeleri

| Madde | Yer |
|---|---|
| Bir grupta bir ülkeden tek takım | `RoundRobinDrawEngine`, sonuç `DrawPlanGuard` ile yeniden doğrulanır |
| A'ya, B'ye, sonra tekrar A'ya | `RoundRobinDrawEngine.Execute` içindeki iç içe iki döngü |
| Bir takım yalnızca bir grupta | Motor ve `(draw_id, team_id)` tekil indeksi |
| Grup sayısı 4 veya 8 | `GroupCount` value object + `CreateDrawCommandValidator` |
| 4 grupta 8 ülke, 8 grupta 4 ülke | `GroupCount.DistinctCountriesPerGroup`, testlerde doğrulanır |
| Kurayı çekenin adı dışarıdan | `CreateDrawRequest` → `ParticipantName` |
| Gruplar ve kurayı çeken kaydedilir | `Draw` aggregate'i, EF Core konfigürasyonları |

## Mimari

```
src/
  Domain          entity, value object, kura motoru (framework bağımlılığı yok)
  Application     command/query, validator, pipeline behavior, DTO
  Infrastructure  EF Core, PostgreSQL, repository, saat, rastgelelik
  Api             controller, hata yönetimi, OpenAPI
tests/
  UnitTests         domain + application, fuzz ve dağılım testleri
  IntegrationTests  gerçek PostgreSQL üzerinde gerçek API
```

Bağımlılıklar yalnızca içeri doğru. Her istek dört MediatR behavior'ından geçer: loglama, validation,
yavaş istek uyarısı, komut başına tek commit atan unit of work. Domain event'leri transaction commit
olduktan sonra yayınlanır. Veritabanında iki tekil indeks kuralları garantiler: `(draw_id, team_id)` bir
takımın iki gruba yazılmasını, `(draw_group_id, selection_order)` bir grup sırasının iki kez
doldurulmasını engeller. Her yerleştirme hangi turda kaçıncı sırada çekildiğini saklar.

## Testler

**90 unit test** value object'leri, validator'ları, behavior'ları ve handler'ları kapsar; kura motorunu
binlerce tohumla çalıştırıp her seferinde tüm kuralları doğrular, iki ki-kare testi de dağılımın eşit
olduğunu ölçer. **26 integration test** container'da PostgreSQL başlatıp gerçek API'yi HTTP üzerinden
sürer, ardından kuralları doğrudan veritabanına sorarak yeniden kontrol eder; on eşzamanlı kura da test
edilir. Uyarılar hata sayılır, derleme geçiyorsa kod da temizdir.

MediatR 12.5.0'a sabit, çünkü 13 ve sonrası ücretli lisans istiyor; mapping de aynı sebeple elle yazıldı.
