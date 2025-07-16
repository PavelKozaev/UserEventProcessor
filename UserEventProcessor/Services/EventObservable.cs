using System.Reactive.Linq;
using System.Reactive.Subjects;
using UserEventProcessor.Models;

namespace UserEventProcessor.Services
{
    public class EventObservable : IDisposable
    {
        private readonly ISubject<UserEvent> _subject = new Subject<UserEvent>();

        public void Publish(UserEvent userEvent)
        {
            _subject.OnNext(userEvent);
        }

        public void Complete()
        {
            _subject.OnCompleted();
        }

        public IObservable<UserEvent> AsObservable()
        {
            return _subject.AsObservable();
        }

        public void Dispose()
        {
            (_subject as IDisposable)?.Dispose();
        }
    }
}
