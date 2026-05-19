// 이 파일은 하위 호환성을 위해 유지됩니다.
// 실제 인터페이스는 MSMES.Application.Common.IJwtService 입니다.
// Infrastructure 내부에서 alias로 사용합니다.

namespace MSMES.Infrastructure.Auth;

/// <summary>
/// Application.Common.IJwtService 를 재-export 하는 alias
/// (Infrastructure 내부 코드가 Application 네임스페이스를 직접 참조하지 않아도 되도록)
/// </summary>
public interface IJwtService : MSMES.Application.Common.IJwtService { }
