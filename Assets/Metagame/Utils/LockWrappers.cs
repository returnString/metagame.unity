using System;
using System.Threading;

namespace Metagame.Utils
{
	public struct ReadLock : IDisposable
	{
		private ReaderWriterLockSlim m_lock;

		public ReadLock(ReaderWriterLockSlim l)
		{
			m_lock = l;
			m_lock.EnterReadLock();
		}

		public void Dispose()
		{
			m_lock.ExitReadLock();
		}
	}

	public struct WriteLock : IDisposable
	{
		private ReaderWriterLockSlim m_lock;

		public WriteLock(ReaderWriterLockSlim l)
		{
			m_lock = l;
			m_lock.EnterWriteLock();
		}

		public void Dispose()
		{
			m_lock.ExitWriteLock();
		}
	}

	public static class LockExtensions
	{
		public static ReadLock Read(this ReaderWriterLockSlim l)
		{
			return new ReadLock(l);
		}

		public static WriteLock Write(this ReaderWriterLockSlim l)
		{
			return new WriteLock(l);
		}
	}
}