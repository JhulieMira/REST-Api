using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DevIO.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class MainController : ControllerBase
    {
        private readonly INotificador _notificador;

        public MainController(INotificador notificador)
        {
            _notificador = notificador;
        }

        protected bool OperacaoValida()
        {
            return !_notificador.TemNotificacao(); //a operação é valida (retorna true)
        }

        protected ActionResult CustomResponse(object result = null) //formatador de resposta personalizado
         {//recebe uma view model e trbalha os erros nela
            if(OperacaoValida()) return Ok(new {success = true, data = result});
            return BadRequest(new {success = false, erros = _notificador.ObterNotificacoes().Select(n => n.Mensagem)});
        }
        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificarErroModelInvalida(modelState);
            return CustomResponse();
        }

        protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
        {
            var erros = modelState.Values.SelectMany(e => e.Errors);
            
            foreach(var erro in erros)
            {
                var errorMessage = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
                NotificarErro(errorMessage);
            }
        }

        protected void NotificarErro(string mensagem)
        {
            _notificador.Handle(new Notificacao(mensagem)); //o handle solta objetos do tipo notificacao, nao tipo string
        }//sera lancado para a fila de erros
    }
}
