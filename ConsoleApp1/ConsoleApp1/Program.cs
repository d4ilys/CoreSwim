namespace ConsoleApp1
{
    internal class Program
    {
        public static void Main()
        {
            var list = new List<string>
            {
                "充值记录ID：6778c2d30077f082031ed1c0，支付分摊金额：658元；充值记录ID：677368ce537b267deefad88b，支付分摊金额：1728元；",
                "充值记录ID：6778bc9fbc367b7f496d86b6，支付分摊金额：1774元；充值记录ID：6778bebcbc367b7f496d8816，支付分摊金额：66元；",
                "充值记录ID：6778f69ea43f0e33452a7f25，支付分摊金额：890元；充值记录ID：677b2d55df0bac9c04bba3cd，支付分摊金额：144元；充值记录ID：677b2eb7512b6e60a186bf6e，支付分摊金额：1.7元；",
                "充值记录ID：677b52f37d5e364ff76c5ffa，支付分摊金额：530元；充值记录ID：677b551a1025931ee90b41a1，支付分摊金额：270元；充值记录ID：677b734ef160f4902d3c10ec，支付分摊金额：200元；充值记录ID：677b73e5d34beadcbc6bffa4，支付分摊金额：92元；"
            };

            var extractor = new LogExtractor();
            var records = extractor.ExtractPaymentInfoFromList(list);

            // 输出提取结果
            foreach (var record in records)
            {
                Console.WriteLine($"ID：{record.RechargeId}，金额：{record.AllocatedAmount}");
            }

            Console.ReadLine();
        }
    }

    /// <summary>
    /// 支付记录信息
    /// </summary>
    public class PaymentRecord
    {
        /// <summary>
        /// 充值记录ID
        /// </summary>
        public string RechargeId { get; set; }

        /// <summary>
        /// 支付分摊金额（元）
        /// </summary>
        public decimal AllocatedAmount { get; set; }
    }


    public class LogExtractor
    {
        /// <summary>
        /// 从字符串集合中提取所有充值记录ID和支付分摊金额
        /// </summary>
        /// <param name="logList">包含日志内容的字符串集合</param>
        /// <returns>所有提取到的有效支付记录</returns>
        public List<PaymentRecord> ExtractPaymentInfoFromList(List<string> logList)
        {
            var result = new List<PaymentRecord>();

            // 校验集合不为空且有元素
            if (logList == null || logList.Count == 0)
                return result;

            // 遍历集合中的每个字符串元素
            foreach (var logItem in logList)
            {
                // 跳过空或空白字符串
                if (string.IsNullOrWhiteSpace(logItem))
                    continue;

                // 处理当前字符串：按分号分割为单条记录（过滤空项）
                var recordParts = logItem.Split(new[] { '；' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => part.Trim())
                    .Where(part => !string.IsNullOrWhiteSpace(part))
                    .ToList();

                // 逐条提取记录信息
                foreach (var part in recordParts)
                {
                    var record = ExtractSingleRecord(part);
                    if (record != null)
                        result.Add(record);
                }
            }

            return result;
        }

        /// <summary>
        /// 从单条记录字符串中提取信息（私有辅助方法）
        /// </summary>
        /// <param name="recordStr">单条记录字符串（如："充值记录ID：xxx，支付分摊金额：xxx元"）</param>
        /// <returns>提取到的记录（无效则返回null）</returns>
        private PaymentRecord ExtractSingleRecord(string recordStr)
        {
            try
            {
                // 提取充值记录ID
                const string idPrefix = "充值记录ID：";
                int idStartIndex = recordStr.IndexOf(idPrefix, StringComparison.Ordinal);
                if (idStartIndex == -1)
                    return null;

                int idValueStart = idStartIndex + idPrefix.Length;
                int idEndIndex = recordStr.IndexOf("，支付分摊金额：", idValueStart, StringComparison.Ordinal);
                if (idEndIndex == -1)
                    return null;

                string rechargeId = recordStr.Substring(idValueStart, idEndIndex - idValueStart).Trim();
                if (string.IsNullOrWhiteSpace(rechargeId))
                    return null;

                // 提取支付分摊金额
                const string amountPrefix = "支付分摊金额：";
                int amountStartIndex = recordStr.IndexOf(amountPrefix, idEndIndex, StringComparison.Ordinal);
                if (amountStartIndex == -1)
                    return null;

                int amountValueStart = amountStartIndex + amountPrefix.Length;
                int amountEndIndex = recordStr.IndexOf("元", amountValueStart, StringComparison.Ordinal);
                if (amountEndIndex == -1)
                    return null;

                string amountStr = recordStr.Substring(amountValueStart, amountEndIndex - amountValueStart).Trim();
                if (!decimal.TryParse(amountStr, out decimal allocatedAmount))
                    return null;

                // 校验通过，返回记录
                return new PaymentRecord
                {
                    RechargeId = rechargeId,
                    AllocatedAmount = allocatedAmount
                };
            }
            catch (Exception ex)
            {
                // 记录异常（不影响其他记录处理）
                Console.WriteLine($"处理单条记录失败：{recordStr}，错误：{ex.Message}");
                return null;
            }
        }
    }
}