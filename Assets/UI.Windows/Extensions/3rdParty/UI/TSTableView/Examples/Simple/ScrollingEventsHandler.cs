using UnityEngine;
using System.Collections;
using Tacticsoft;

namespace Tacticsoft.Examples
{
    public class ScrollingEventsHandler : MonoBehaviour
    {
        public TableView m_tableView;

        public void ScrollToTopAnimated() {
            StartCoroutine(AnimateToScrollY(0, 2f));
        }

		public void ScrollToBottomImmediate() {
			m_tableView.scrollY = m_tableView.scrollableHeight;
		}

        public void ScrollToRow10Animated() {
            float scrollY = m_tableView.GetScrollYForRow(10, true);
            StartCoroutine(AnimateToScrollY(scrollY, 2f));
        }

        //In practice, it is better to use libraries such as iTween to animate TableView's scrollY
        //This example uses a hard coded animator to keep dependencies down
        private IEnumerator AnimateToScrollY(float scrollY, float time) {
            float startTime = Time.time;
            float startScrollY = m_tableView.scrollY;
            float endTime = startTime + time;
            while (Time.time < endTime) {
                float relativeProgress = Mathf.InverseLerp(startTime, endTime, Time.time);
                m_tableView.scrollY = Mathf.Lerp(startScrollY, scrollY, relativeProgress);
                yield return new WaitForEndOfFrame();
            }
            m_tableView.scrollY = scrollY;
        }

        
    }

}
